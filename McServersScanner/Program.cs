using CommandLine;
using CommunityToolkit.HighPerformance.Buffers;
using McServersScanner;
using McServersScanner.Utils;
using System.Net;
using System.Threading.Tasks.Dataflow;

internal class Program
{
    private static async Task Main(string[] args)
    {
        ScannerConfiguration config = new();

        //Parsing cmd params
        ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args);

        try
        {
            parseResult(config, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        if (result.Errors.Any())
            return;

        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

        Scanner.ApplyConfiguration(config);

        await Scanner.Scan();
    }

    private static void parseResult(ScannerConfiguration config, ParserResult<Options> result)
    {
        result.WithParsed(o =>
        {
            //Adding connection limit
            int? conLimit = o.ConnectionLimit;

            if (conLimit is not null)
                config.connectionLimit = conLimit.Value;

            //Adding ports
            List<string>? portList = o.Ports?.ToList();

            if (portList is not null)
            {
                List<ushort> portUshot = new();

                foreach (string portString in portList)
                {
                    if (portString.Contains('-')) //ports range
                    {
                        string[] splittedPorts = portString.Split('-');

                        var range = NetworkHelper.FillPortRange(splittedPorts[0], splittedPorts[1]);
                        foreach (ushort port in range)
                            portUshot.Add(port);
                    }
                    else //single port
                    {
                        if (portString.Any(c => char.IsLetter(c)))
                            throw new FormatException("Option \'p\' have wrong format");

                        config.ports = new ushort[] { ushort.Parse(portString) };
                    }
                }
            }

            //Adding ips
            config.ips = new(new DataflowBlockOptions()
            {
                BoundedCapacity = config.connectionLimit ?? Scanner.ConnectionLimit
            });

            int portsCount = config.ports?.Length ?? Scanner.PortsCount;

            List<string> ipOptionRange = o.Range!.ToList();

            foreach (string ipOption in ipOptionRange)
            {
                if (ipOption.Contains('-')) //ip range
                {
                    string[] splittedIps = ipOption.Split('-');

                    string firstIp = StringPool.Shared.GetOrAdd(splittedIps[0]);
                    string lastIp = StringPool.Shared.GetOrAdd(splittedIps[1]);

                    config.totalIps = NetworkHelper.GetIpRangeCount(firstIp, lastIp) * portsCount;

                    var range = NetworkHelper.FillIpRange(firstIp, lastIp);

                    config.addIpAdresses = Task.Run(() => Scanner.CopyToActionBlockAsync(range, config.ips));
                }
                else if (ipOption.Where(x => char.IsLetter(x)).Count() > 0) //ips from file
                {
                    config.totalIps = IOHelper.GetLinesCount(ipOption) * portsCount;

                    var readedIps = IOHelper.ReadLineByLine(ipOption);

                    config.addIpAdresses = Task.Run(() => Scanner.CopyToActionBlockAsync(from ip in readedIps select IPAddress.Parse(ip), config.ips));
                }
                else //single ip
                {
                    config.totalIps = portsCount;

                    config.ips.Post(IPAddress.Parse(ipOption));
                }
            }

            //Adding connection timeout
            double? connectionTimeout = o.ConnectionTimeout;

            if (connectionTimeout is not null)
                config.timeout = connectionTimeout.Value;

        });
    }

    /// <summary>
    /// Save progress on programm interruption (Ctrl+C)
    /// </summary>
    private static void OnExit(object? sender, ConsoleCancelEventArgs e)
    {
        Scanner.ForceStop();
        Environment.Exit(0);
    }
}
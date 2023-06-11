using System.Net;
using System.Threading.Tasks.Dataflow;
using CommandLine;
using CommunityToolkit.HighPerformance.Buffers;
using McServersScanner.Core;
using McServersScanner.Core.Utils;

namespace McServersScanner;

internal class Program
{
    public static async Task Main(string[] args)
    {
        ScannerConfiguration config;

        //Parsing cmd params
        ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args);

        try
        {
            config = parseResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        if (result.Errors.Any())
            return;

        Console.CancelKeyPress += OnExit;

        Scanner.ApplyConfiguration(config);

        await Scanner.Scan();
    }

    private static ScannerConfiguration parseResult(ParserResult<Options> result)
    {
        ScannerConfiguration config = new();

        result.WithParsed(o =>
        {
            //Adding connection limit
            int? conLimit = o.ConnectionLimit;

            if (conLimit is not null)
                config.ConnectionLimit = conLimit.Value;

            //Adding bandwidth limit
            string? bandwidthLimit = o.BandwidthLimit;

            if (!string.IsNullOrEmpty(bandwidthLimit))
                config.BandwidthLimit = ParsingHelper.ConvertToNumberOfBytes(bandwidthLimit);

            //Adding ports
            List<string>? portList = o.Ports?.ToList();

            if (portList?.Count > 0)
            {
                List<ushort> portUshort = new();

                foreach (string portString in portList)
                {
                    if (portString.Contains('-')) //ports range
                    {
                        string[] splitPorts = portString.Split('-');

                        var range = NetworkHelper.FillPortRange(splitPorts[0], splitPorts[1]);
                        foreach (ushort port in range)
                            portUshort.Add(port);
                    }
                    else //single port
                    {
                        if (portString.Any(char.IsLetter))
                            throw new FormatException("Option \'p\' have wrong format");

                        portUshort.Add(ushort.Parse(portString));
                    }
                }

                config.Ports = portUshort.ToArray();
            }

            //Adding ips
            config.Ips = new(new DataflowBlockOptions()
            {
                BoundedCapacity = config.ConnectionLimit ?? Scanner.ConnectionLimit
            });

            int portsCount = config.Ports?.Length ?? Scanner.PortsCount;

            List<string> ipOptionRange = o.Range!.ToList();

            foreach (string ipOption in ipOptionRange)
            {
                if (ipOption.Contains('-')) //ip range
                {
                    string[] splitIps = ipOption.Split('-');

                    string firstIp = StringPool.Shared.GetOrAdd(splitIps[0]);
                    string lastIp = StringPool.Shared.GetOrAdd(splitIps[1]);

                    config.TotalIps = NetworkHelper.GetIpRangeCount(firstIp, lastIp) * portsCount;

                    var range = NetworkHelper.FillIpRange(firstIp, lastIp);

                    config.AddIpAddresses = Task.Run(() => Scanner.CopyToActionBlockAsync(range, config.Ips));
                }
                else if (ipOption.Any(char.IsLetter)) //ips from file
                {
                    config.TotalIps = IOHelper.GetLinesCount(ipOption) * portsCount;

                    var readIps = IOHelper.ReadLineByLine(ipOption);

                    config.AddIpAddresses = Task.Run(() =>
                        Scanner.CopyToActionBlockAsync(from ip in readIps select IPAddress.Parse(ip), config.Ips));
                }
                else //single ip
                {
                    config.TotalIps = portsCount;

                    config.Ips.Post(IPAddress.Parse(ipOption));
                }
            }

            //Adding connection timeout
            double? connectionTimeout = o.ConnectionTimeout;

            if (connectionTimeout is not null)
                config.Timeout = connectionTimeout.Value;
        });

        return config;
    }

    /// <summary>
    /// Save progress on program interruption (Ctrl+C)
    /// </summary>
    public static void OnExit(object? sender, ConsoleCancelEventArgs e)
    {
        Scanner.ForceStop();
        Environment.Exit(0);
    }
}
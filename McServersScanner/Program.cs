using System.Net;
using System.Threading.Tasks.Dataflow;
using CommandLine;
using CommunityToolkit.HighPerformance.Buffers;
using McServersScanner.Core;
using McServersScanner.Core.Utils;
using Serilog;

namespace McServersScanner;

internal static class Program
{
    private static Scanner? scanner;

    public static async Task Main(string[] args)
    {
        Console.CancelKeyPress += OnExit;

        ScannerBuilder scannerBuilder = new();

        //Parsing cmd params
        ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args);

        if (result.Errors.Any())
            return;

        scannerBuilder.ConfigureDefaultLogger();

        try
        {
            transferParseResultsToBuilder(result, scannerBuilder);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Parameters parsing thrown an exception");
            return;
        }

        scanner = scannerBuilder.Build();

        await scanner.Scan();
    }

    private static void transferParseResultsToBuilder(ParserResult<Options> result, ScannerBuilder builder) =>
        result.WithParsed(o =>
        {
            //Adding connection limit
            if (o.ConnectionLimit.HasValue)
                builder.ConnectionLimit = o.ConnectionLimit.Value;

            //Adding bandwidth limit
            string? bandwidthLimit = o.BandwidthLimit;

            if (!string.IsNullOrEmpty(bandwidthLimit))
                builder.BandwidthLimit = ParsingHelper.ConvertToNumberOfBytes(bandwidthLimit);

            //Adding Ports
            List<string>? portList = o.Ports?.ToList();

            if (portList?.Count > 0)
            {
                List<ushort> portUshort = new();

                foreach (string portString in portList)
                    if (portString.Contains('-')) //Ports range
                    {
                        string[] splitPorts = portString.Split('-');

                        IEnumerable<ushort>? range = NetworkHelper.FillPortRange(splitPorts[0], splitPorts[1]);
                        foreach (ushort port in range)
                            portUshort.Add(port);
                    }
                    else //single port
                    {
                        if (portString.Any(char.IsLetter))
                            throw new FormatException("Option \'p\' have wrong format");

                        portUshort.Add(ushort.Parse(portString));
                    }

                builder.Ports = portUshort.ToArray();
            }

            //Adding ips
            builder.Ips = new BufferBlock<IPAddress>(new DataflowBlockOptions()
            {
                BoundedCapacity = builder.ConnectionLimit
            });

            string[] ipOptionRange = o.Range!.ToArray();

            foreach (string ipOption in ipOptionRange)
                if (ipOption.Contains('-')) //ip range
                {
                    string[] splitIps = ipOption.Split('-');

                    string firstIp = StringPool.Shared.GetOrAdd(splitIps[0]);
                    string lastIp = StringPool.Shared.GetOrAdd(splitIps[1]);

                    builder.IpsCount = NetworkHelper.GetIpRangeCount(firstIp, lastIp);

                    IEnumerable<IPAddress>? range = NetworkHelper.FillIpRange(firstIp, lastIp);

                    builder.AddIpAddresses = Task.Run(() => Scanner.CopyToBufferBlockAsync(range, builder.Ips));
                }
                else if (ipOption.Any(char.IsLetter)) //ips from file
                {
                    builder.IpsCount = IOHelper.GetLinesCount(ipOption);

                    IEnumerable<string>? readIps = IOHelper.ReadLineByLine(ipOption);

                    builder.AddIpAddresses = Task.Run(() =>
                        Scanner.CopyToBufferBlockAsync(from ip in readIps select IPAddress.Parse(ip), builder.Ips));
                }
                else //single ip
                {
                    builder.IpsCount = 1;

                    builder.Ips.Post(IPAddress.Parse(ipOption));
                }

            //Adding connection timeout
            if (o.ConnectionTimeout.HasValue)
                builder.Timeout = o.ConnectionTimeout.Value;
        });

    /// <summary>
    /// Save progress on program interruption (Ctrl+C)
    /// </summary>
    public static void OnExit(object? sender, ConsoleCancelEventArgs e)
    {
        scanner?.ForceStop();
        Environment.Exit(0);
    }
}
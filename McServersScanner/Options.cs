using CommandLine;

namespace McServersScanner;

/// <summary>
/// Stores all possible options for command line
/// </summary>
public class Options
{
    [Option('I', "range", Required = true, HelpText = "Ip address range to be processed.")]
    public IEnumerable<string>? Range { get; init; }

    [Option('p', "port", Required = false, HelpText = "Range of Ports to be scanned.")]
    public IEnumerable<string>? Ports { get; init; }

    [Option("connection-limit", HelpText = "Max amount of connections at the same time")]
    public int? ConnectionLimit { get; init; }

    [Option('b', "bandwidth-limit", HelpText = "Maximum number of bytes send/received by network per second")]
    public string? BandwidthLimit { get; init; }

    [Option('t', "connection-timeout", HelpText = "Timeout option (in seconds)")]
    public double? ConnectionTimeout { get; init; }
}
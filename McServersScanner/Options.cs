using CommandLine;

namespace McServersScanner
{
    /// <summary>
    /// Stores all possible options for command line
    /// </summary>
    public class Options
    {
        [Option('I', "range", Required = true, HelpText = "Ip address range to be processed.")]
        public IEnumerable<string>? Range { get; set; }

        [Option('p', "port", Required = false, HelpText = "Range of ports to be scanned.")]
        public IEnumerable<string>? Ports { get; set; }

        [Option("connection-limit", HelpText = "Max amount of connections at the same time")]
        public int? ConnectionLimit { get; set; }

        [Option('b', "bandwidth-limit", HelpText = "Maximum number of bytes send/received by network per second")]
        public string? BandwidthLimit { get; set; }

        [Option('t', "connection-timeout", HelpText = "Timeout option (in seconds)")]
        public double? ConnectionTimeout { get; set; }
    }
}

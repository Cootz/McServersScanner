using CommandLine;

namespace McServersScanner.CLI
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
    }
}

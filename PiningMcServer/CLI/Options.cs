using CommandLine;

namespace McServersScanner.CLI
{
    /// <summary>
    /// Stores all possible options for command line
    /// </summary>
    public class Options
    {
        [Option('I', "range", Required = true, HelpText = "Ip adress range to be processed.")]
        public IEnumerable<string>? Range { get; set; }
    }
}

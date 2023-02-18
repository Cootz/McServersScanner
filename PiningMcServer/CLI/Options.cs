using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McServersScanner.CLI
{
    public class Options
    {
        [Option('I', "range", Required = true, HelpText = "Ip adress range to be processed.")]
        public IEnumerable<string>? Range { get; set; }
    }
}

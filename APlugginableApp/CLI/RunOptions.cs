using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace APlugginableApp.CLI
{
    [Verb("run", HelpText = "Executes a selected plugin")]
    class RunOptions
    {
        [Option('p', "plugin")]
        public string Plugin { get; set; }

        [Option('a', "argument")]
        public string Argument { get; set; }
    }
}

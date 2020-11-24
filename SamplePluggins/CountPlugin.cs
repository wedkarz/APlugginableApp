using System;
using System.Collections.Generic;
using System.Text;

namespace APlugginableApp.Plugins
{
    class CountPlugin : IPlugin
    {
        public string Description => "Counts characters in input string";

        public string Execute(string input)
        {
            return $"{input.Length}";
        }
    }
}

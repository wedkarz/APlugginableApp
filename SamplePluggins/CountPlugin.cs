using System;
using System.Collections.Generic;
using System.Text;

namespace APlugginableApp.Plugins
{
    class CountPlugin : IPlugin
    {
        public string Execute(string input)
        {
            return $"{input.Length}";
        }
    }
}

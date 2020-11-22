using System;
using System.Collections.Generic;
using System.Text;

namespace APlugginableApp.Plugins
{
    class ReversePlugin : IPlugin
    {
        public string Execute(string input)
        {
            var characters = input.ToCharArray();
            Array.Reverse(characters);
            return new string(characters);
        }
    }
}

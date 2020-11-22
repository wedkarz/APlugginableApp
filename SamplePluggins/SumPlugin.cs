using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace APlugginableApp.Plugins
{
    class SumPlugin : IPlugin
    {
        public string Execute(string input)
        {
            var substrings = input.Split('+').Select(s => s.Trim());

            double sum = 0;
            foreach(var numberCandidate in substrings)
            {
                if(double.TryParse(numberCandidate, out double number))
                {
                    sum += number;
                } else
                {
                    return $"Invalid argument: {numberCandidate}";
                }
            }
            
            return $"{sum}";
        }
    }
}

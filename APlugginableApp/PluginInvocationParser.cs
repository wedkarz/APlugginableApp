using APlugginableApp.CLIv2;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace APlugginableApp
{
    class PluginInvocationParser: IPluginInvocationParser
    {
        private readonly IPluginProvider _pluginProvider;

        public PluginInvocationParser(IPluginProvider pluginProvider)
        {
            _pluginProvider = pluginProvider;
        }

        public PluginInvocationDescriptor ParseCommand(string commandString)
        {
            string methodPattern = @"(?<pluginName>\w+)\s*\(""(?<pluginArgument>.*)""\)";
            Regex r = new Regex(methodPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match m = r.Match(commandString.Replace(Environment.NewLine, ""));

            if (!m.Success)
            {
                return null;
            }

            var pluginName = m.Groups["pluginName"].Value;
            var pluginArgument = m.Groups["pluginArgument"].Value;

            var availablePlugins = _pluginProvider.ProvidePlugins();
            var matchedPluggin = availablePlugins.FirstOrDefault(p => p.Name.ToLower() == pluginName.ToLower());

            if (matchedPluggin == null)
            {
                return null;
            }

            return new PluginInvocationDescriptor(matchedPluggin, pluginArgument);
        }
    }
}

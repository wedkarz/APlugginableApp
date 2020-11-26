using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace APlugginableApp.CLIv2
{
    [Command(Name = "list", OptionsComparison = StringComparison.InvariantCultureIgnoreCase, Description = "Lists available plugins")]
    internal class ListPluginsCmd: ApaCLIBaseCmd
    {
        private readonly IPluginProvider _pluginProvider;

        public ListPluginsCmd(IPluginProvider pluginProvider, ILogger<ListPluginsCmd> logger, IConsole console): base(logger, console)
        {
            _pluginProvider = pluginProvider;
        }

        override protected Task<int> OnExecute(CommandLineApplication app)
        {
            var plugins = _pluginProvider.ProvidePlugins();
            var pluginsListString = string.Join(Environment.NewLine, plugins.Select(p => $"{p.Name}\t\t{p.Description}"));

            OutputToConsole(pluginsListString + Environment.NewLine);
            return Task.FromResult(0);
        }
    }
}
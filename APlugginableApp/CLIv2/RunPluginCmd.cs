using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APlugginableApp.CLIv2
{
    [Command(Name = "run", OptionsComparison = StringComparison.InvariantCultureIgnoreCase, Description = "Runs single plugin with argument")]
    internal class RunPluginCmd : ApaCLIBaseCmd
    {
        private readonly IPluginProvider _pluginProvider;

        public RunPluginCmd(IPluginProvider pluginProvider, ILogger<RunPluginCmd> logger, IConsole console) : base(logger, console)
        {
            _pluginProvider = pluginProvider;
        }

        [Option("-p")]
        [Required]
        public string Plugin { get; }

        [Option("-a")]
        [Required]
        public string Argument { get; }

        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            var plugins = _pluginProvider.ProvidePlugins();
            var plugin = plugins.FirstOrDefault(p => p.Name.ToLower() == Plugin.ToLower());

            if (plugin == null)
            {
                OutputError($"Plugin {Plugin} not supported. Check available plugins by running: apa list");
                return Task.FromResult(-1);
            }

            OutputToConsole(plugin.Execute(Argument));
            return Task.FromResult(0);
        }
    }
}
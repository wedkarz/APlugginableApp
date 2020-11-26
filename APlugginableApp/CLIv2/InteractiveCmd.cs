using Cintio;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APlugginableApp.CLIv2
{
    [Command(Name = "interactive", OptionsComparison = StringComparison.InvariantCultureIgnoreCase, Description = "Runs interactive interpreter aka. REPL")]
    class InteractiveCmd: ApaCLIBaseCmd
    {
        private readonly IPluginInvocationParser _pluginInvocationParser;
        private readonly IPluginProvider _pluginProvider;

        public InteractiveCmd(IPluginInvocationParser pluginInvocationParser, IPluginProvider pluginProvider, ILogger<InteractiveCmd> logger, IConsole console): base(logger, console)
        {
            _pluginInvocationParser = pluginInvocationParser;
            _pluginProvider = pluginProvider;
        }

        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            RunREPL();
            return Task.FromResult(0);
        }

        private void RunREPL()
        {
            var prompt = "apa> ";
            var compList = _pluginProvider.ProvidePlugins().Select(p => p.Name).ToList();
            var startupMsg = "Enter command to execute. Type exit to exit.";
            InteractivePrompt.Run(
                ((strCmd, listCmd, completions) =>
                {
                    if (strCmd.ToLower() == "exit")
                    {
                        Process.GetCurrentProcess().Kill();
                    }

                    var command = _pluginInvocationParser.ParseCommand(strCmd);
                    if (command == null)
                    {                        
                        return $"{strCmd} is not supported {Environment.NewLine}Remember to call plugin with argument. Try: {strCmd}(\"Test Data\"){Environment.NewLine}";
                    }

                    return $"{command.Plugin.Execute(command.Argument)} {Environment.NewLine}";
                }), prompt, startupMsg, compList);
        }
    }
}

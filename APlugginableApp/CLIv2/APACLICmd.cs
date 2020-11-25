using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace APlugginableApp.CLIv2
{
    [Command(Name = "apa", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(ListPluginsCmd),
        typeof(RunPluginCmd),
        typeof(InteractiveCmd))]
    class APACLICmd: ApaCLIBaseCmd
    {
        public APACLICmd(ILogger<ApaCLIBaseCmd> logger, IConsole console) : base(logger, console)
        {
        }

        private static string GetVersion()
            => typeof(APACLICmd).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

    }
}

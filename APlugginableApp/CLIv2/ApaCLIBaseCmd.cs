using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace APlugginableApp.CLIv2
{
    abstract class ApaCLIBaseCmd
    {
        protected readonly ILogger _logger;
        //protected IHttpClientFactory _httpClientFactory;
        protected readonly IConsole _console;

        protected ApaCLIBaseCmd(ILogger logger, IConsole console)
        {
            _logger = logger;
            _console = console;
        }

        virtual protected Task<int> OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return Task.FromResult(0);
        }

        protected void OnException(Exception ex)
        {
            OutputError(ex.Message);
            _logger.LogError(ex.Message);
            _logger.LogDebug(ex, ex.Message);
        }

        protected void OutputToConsole(string data)
        {
            //_console.BackgroundColor = ConsoleColor.Black;
            //_console.ForegroundColor = ConsoleColor.White;
            _console.Out.Write(data);
            //_console.ResetColor();
        }

        protected void OutputError(string message)
        {
            _console.BackgroundColor = ConsoleColor.Red;
            _console.ForegroundColor = ConsoleColor.White;
            _console.Error.WriteLine(message);
            _console.ResetColor();
        }
    }
}

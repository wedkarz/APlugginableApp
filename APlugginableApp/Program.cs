using APlugginableApp.CLIv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APlugginableApp
{
    class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var Configuration = new ConfigurationBuilder()
                .AddJsonFile("applicationSettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(Configuration)
                   .Enrich.FromLogContext()
                   .CreateLogger();

            var serilogLogger = new SerilogLoggerProvider(Log.Logger);

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(config =>
                    {
                        config.ClearProviders();
                        config.AddProvider(serilogLogger);
                    });

                    services.AddOptions();
                    services.Configure<LifecycleSettings>(Configuration.GetSection("Lifecycle"));

                    services.AddSingleton<IPluginProvider, PluginProvider>();
                    services.AddSingleton<IPluginInvocationParser, PluginInvocationParser>();
                    //services.AddSingleton<IAPAConfiguration>((serviceProvider) => { return ApplicationConfig.ReadFrom(Configuration); });
                });

            try
            {
                return await builder.RunCommandLineApplicationAsync<APACLICmd>(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}

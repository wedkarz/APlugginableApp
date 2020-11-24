using APlugginableApp.CLI;
using Cintio;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;

namespace APlugginableApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ListOptions, RunOptions, InteractiveOptions>(args)
                .WithParsed<ListOptions>(DoListOptions)
                .WithParsed<RunOptions>(DoRunOptions)
                .WithParsed<InteractiveOptions>(DoInteractiveOptions)
                .WithNotParsed(HandleErrors);
        }

        private static void DoInteractiveOptions(InteractiveOptions obj)
        {
            var plugins = GetAvailablePlugins();
            RunREPL(plugins);
        }

        private static void RunREPL(IEnumerable<PluginDescriptor> plugins)
        {
            var prompt = "apa> ";
            var compList = plugins.Select(p => p.ExecCommand).ToList();
            var startupMsg = "Enter command to execute";
            InteractivePrompt.Run(
                ((strCmd, listCmd, completions) =>
                {
                    var command = ParseCommand(strCmd);
                    if (command == null)
                    {
                        return $"{strCmd} is not supported {Environment.NewLine}";
                    }

                    return $"{command.Plugin.Execute(command.Argument)} {Environment.NewLine}";
                }), prompt, startupMsg, compList);
        }

        private static CommandDescriptor ParseCommand(string commandString)
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

            var availablePlugins = GetAvailablePlugins();
            var matchedPluggin = availablePlugins.FirstOrDefault(p => p.ExecCommand.ToLower() == pluginName.ToLower());

            if (matchedPluggin == null)
            {
                return null;
            }

            return new CommandDescriptor(matchedPluggin, pluginArgument);
        }

        private static void HandleErrors(IEnumerable<Error> obj)
        {
        }

        private static void DoListOptions(ListOptions args)
        {
            var plugins = GetAvailablePlugins();
            Array.ForEach(plugins.Select(p => $"{p.ExecCommand}\t\t{p.Description}").ToArray(), Console.WriteLine);
        }

        private static void DoRunOptions(RunOptions args)
        {
            if (args.Plugin == null || args.Plugin.Length == 0)
            {
                Console.WriteLine("No plugins specified.\n\nUsage: apa run -p [plugin] -a [argument]\n");
                return;
            }

            if (args.Argument == null || args.Argument.Length == 0)
            {
                Console.WriteLine($"No arguments specified for plugin: {args.Plugin}\n\nUsage: apa run -p {args.Plugin} -a [argument]\n");
                return;
            }

            var plugins = GetAvailablePlugins();
            var plugin = plugins.FirstOrDefault(p => p.ExecCommand.ToLower() == args.Plugin.ToLower());
            if (plugin == null)
            {
                Console.WriteLine($"Plugin {plugin} not supported");
                return;
            }

            Console.WriteLine(plugin.Execute(args.Argument));
        }

        private static IEnumerable<PluginDescriptor> GetAvailablePlugins()
        {
            var exeDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pluginsPath = Path.Combine(exeDirectoryPath, "Pluggins");
            if (!Directory.Exists(pluginsPath))
            {
                return new List<PluginDescriptor>();
            }

            var potentialPlugginFilePaths = Directory.GetFiles(pluginsPath, "*.dll");
            var potentialPlugginAssemblies = potentialPlugginFilePaths.Select(LoadAssembly).Where(a => a != null);

            var plugins = potentialPlugginAssemblies.Where(ValidatePluginInterface).SelectMany(ExtractPluggins);

            return plugins;
        }

        private static bool ValidatePluginInterface(Assembly assembly)
        {
            var pluginInterface = assembly.GetTypes().FirstOrDefault(t => t.Name == "IPlugin");
            if (pluginInterface == null) return false;

            var plugginInterfaceExecuteMethod = pluginInterface.GetMethod("Execute");
            if (plugginInterfaceExecuteMethod == null) return false;

            var plugginInterfaceExecuteMethodParams = plugginInterfaceExecuteMethod.GetParameters();
            var isExecuteMethodMatchingParams = plugginInterfaceExecuteMethodParams.Length == 1 && plugginInterfaceExecuteMethodParams[0].ParameterType == typeof(string);
            var isExecuteMethodMatchingReturnType = plugginInterfaceExecuteMethod.ReturnType == typeof(string);

            var pluginInterfaceDescriptionProperty = pluginInterface.GetProperty("Description");
            if (pluginInterfaceDescriptionProperty == null) return false;
            var isDescriprionPropertyValid = pluginInterfaceDescriptionProperty.PropertyType == typeof(string) && pluginInterfaceDescriptionProperty.CanRead;

            return isExecuteMethodMatchingParams && isExecuteMethodMatchingReturnType && isDescriprionPropertyValid;
        }


        private static IEnumerable<PluginDescriptor> ExtractPluggins(Assembly assembly)
        {
            var pluginTypePrefix = "Plugin";
            var pluginInterfaceType = assembly.GetTypes().FirstOrDefault(t => t.Name == "IPlugin");

            var plugginDescriptors = assembly.GetTypes()
                .Where(t => t.GetInterface("IPlugin") != null)
                .Select(p => new PluginDescriptor(p, p.Name.Replace(pluginTypePrefix, "")));

            return plugginDescriptors;
        }

        private static Assembly LoadAssembly(string path)
        {
            PluginLoadContext loadContext = new PluginLoadContext(path);
            try
            {
                return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
            } catch(Exception e)
            {
                return null;
            }
        }
    }


    internal class CommandDescriptor
    {
        internal PluginDescriptor Plugin { get; }
        internal string Argument { get; }

        internal CommandDescriptor(PluginDescriptor pluginDescriptor, string argument)
        {
            Plugin = pluginDescriptor;
            Argument = argument;
        }
    }

    internal class PluginDescriptor {
        private Type _pluginType;

        internal string ExecCommand { get; }


        private object _pluginInstance = null;
        private object PluginInstance {
            get { 
                if(_pluginInstance == null)
                {
                    _pluginInstance = Activator.CreateInstance(_pluginType);
                }

                return _pluginInstance;
            } 
        }

        private string _description = null;
        internal string Description { 
            get {
                if (_description == null)
                {
                    var descriptionPropertyInfo = _pluginType.GetProperty("Description");
                    _description = (string)descriptionPropertyInfo.GetValue(PluginInstance);
                }

                return _description;
            } 
        }

        internal string Execute(string input)
        {
            var methodInfo = _pluginType.GetMethod("Execute");
            var pluginObject = Activator.CreateInstance(_pluginType);

            var output = methodInfo.Invoke(pluginObject, new[] { input });
            if(output is string)
            {
                return output as string;
            }

            return string.Empty;
        }

        internal PluginDescriptor(Type pluginType, string name)
        {
            _pluginType = pluginType;
            ExecCommand = name;
        }
    }

    class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                try
                {
                    return LoadFromAssemblyPath(assemblyPath);
                } catch(Exception e)
                {
                    return null;
                }
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}

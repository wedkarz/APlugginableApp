using APlugginableApp.CLI;
using Cintio;
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace APlugginableApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var plugins = GetAvailablePlugins();

            var prompt = "apa> ";
            var compList = plugins.Select(p => p.ExecCommand).ToList();
            var startupMsg = "Enter command to execute";
            InteractivePrompt.Run(
                ((strCmd, listCmd, completions) =>
                {
                    foreach (var c in strCmd.Split(' '))
                        if (!completions.Contains(c))
                            completions.Add(c);

                    var command = ParseCommand(strCmd);
                    if (command == null)
                    {
                        return $" >>> {strCmd} not supported<<< {Environment.NewLine}";
                    }

                    return $"{command.Plugin.Execute(command.Argument)}  {Environment.NewLine}";
                }), prompt, startupMsg, compList);

            Parser.Default.ParseArguments<ListOptions, RunOptions>(args)
                .WithParsed<ListOptions>(DoListOptions)
                .WithParsed<RunOptions>(DoRunOptions)
                .WithNotParsed(HandleErrors);
        }

        private static CommandDescriptor ParseCommand(string commandString)
        {
            string methodPattern = @"(?<pluginName>\w+)\s*\(""(?<pluginArgument>.*)""\)";
            Regex r = new Regex(methodPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match m = r.Match(commandString.Replace(Environment.NewLine, ""));
            
            if(!m.Success)
            {
                return null;
            }

            var pluginName = m.Groups["pluginName"].Value;
            var pluginArgument = m.Groups["pluginArgument"].Value;

            var availablePlugins = GetAvailablePlugins();
            var matchedPluggin = availablePlugins.FirstOrDefault(p => p.ExecCommand.ToLower() == pluginName.ToLower());

            if(matchedPluggin == null)
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
            Console.WriteLine("List of available plugins: ");
            Array.ForEach(plugins.ToArray(), Console.WriteLine);
        }

        private static void DoRunOptions(RunOptions args)
        {

        }

        private static IEnumerable<PluginDescriptor> GetAvailablePlugins()
        {
            var exeDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pluginsPath = Path.Combine(exeDirectoryPath, "Pluggins");
            if(!Directory.Exists(pluginsPath))
            {
                return new List<PluginDescriptor>();
            }

            var potentialPlugginFilePaths = Directory.GetFiles(pluginsPath, "*.dll");
            var potentialPlugginAssemblies = potentialPlugginFilePaths.Select(LoadAssembly);

            var pluginTypePrefix = "Plugin";

            var plugins = potentialPlugginAssemblies.Where(DefinesPluggins).SelectMany(ExtractPluggins);

            return plugins;
        }

        private static bool DefinesPluggins(Assembly assembly)
        {
            var pluginInterface = assembly.GetTypes().FirstOrDefault(t => t.Name == "IPlugin");
            if (pluginInterface == null) return false;

            var plugginInterfaceExecuteMethod = pluginInterface.GetMethod("Execute");
            if (plugginInterfaceExecuteMethod == null) return false;

            var plugginInterfaceExecuteMethodParams = plugginInterfaceExecuteMethod.GetParameters();
            var isExecuteMethodMatchingParams = plugginInterfaceExecuteMethodParams.Length == 1 && plugginInterfaceExecuteMethodParams[0].ParameterType == typeof(string);
            var isExecuteMethodMatchingReturnType = plugginInterfaceExecuteMethod.ReturnType == typeof(string);

            return isExecuteMethodMatchingParams && isExecuteMethodMatchingReturnType;
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
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
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
                return LoadFromAssemblyPath(assemblyPath);
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

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace APlugginableApp
{
    internal class PluginProvider : IPluginProvider
    {
        private readonly string _pluginsDirectory = "Plugins";
        private readonly string _pluginInterfaceType = "IPlugin";
        private readonly string _pluginInterfaceExecuteMethodName = "Execute";
        private readonly string _pluginInterfaceDescriptionPropertyName = "Description";
        private readonly string _pluginNamePrefix = "Plugin";

        private readonly IOptions<LifecycleSettings> _lifecycleSettings;
        public PluginProvider(IOptions<LifecycleSettings> lifecycleSettings)
        {
            _lifecycleSettings = lifecycleSettings;
        }

        public IEnumerable<PluginDescriptor> ProvidePlugins()
        {
            var exeDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pluginsPath = Path.Combine(exeDirectoryPath, _pluginsDirectory);
            if (!Directory.Exists(pluginsPath))
            {
                return new List<PluginDescriptor>();
            }

            var potentialPlugginFilePaths = Directory.GetFiles(pluginsPath, "*.dll");
            var potentialPlugginAssemblies = potentialPlugginFilePaths.Select(LoadAssembly).Where(a => a != null);

            var plugins = potentialPlugginAssemblies.Where(ValidatePluginInterface).SelectMany(ExtractPluggins);

            return plugins;
        }

        private bool ValidatePluginInterface(Assembly assembly)
        {
            var pluginInterface = assembly.GetTypes().FirstOrDefault(t => t.Name == typeof(IPlugin).Name);
            if (pluginInterface == null) return false;

            var plugginInterfaceExecuteMethod = pluginInterface.GetMethod(_pluginInterfaceExecuteMethodName);
            if (plugginInterfaceExecuteMethod == null) return false;

            var plugginInterfaceExecuteMethodParams = plugginInterfaceExecuteMethod.GetParameters();
            var isExecuteMethodMatchingParams = plugginInterfaceExecuteMethodParams.Length == 1 && plugginInterfaceExecuteMethodParams[0].ParameterType == typeof(string);
            var isExecuteMethodMatchingReturnType = plugginInterfaceExecuteMethod.ReturnType == typeof(string);

            var pluginInterfaceDescriptionProperty = pluginInterface.GetProperty(_pluginInterfaceDescriptionPropertyName);
            if (pluginInterfaceDescriptionProperty == null) return false;
            var isDescriprionPropertyValid = pluginInterfaceDescriptionProperty.PropertyType == typeof(string) && pluginInterfaceDescriptionProperty.CanRead;

            return isExecuteMethodMatchingParams && isExecuteMethodMatchingReturnType && isDescriprionPropertyValid;
        }


        private IEnumerable<PluginDescriptor> ExtractPluggins(Assembly assembly)
        {
            var pluginInterfaceType = assembly.GetTypes().FirstOrDefault(t => t.Name == _pluginsDirectory);

            var plugginDescriptors = assembly.GetTypes()
                .Where(t => t.GetInterface(_pluginInterfaceType) != null)
                .Select(p => new PluginDescriptor(p, p.Name.Replace(_pluginNamePrefix, ""), _lifecycleSettings.Value.InstanceCreation));

            return plugginDescriptors;
        }

        private Assembly LoadAssembly(string path)
        {
            var loadContext = new PluginLoadContext(path);

            try
            {
                return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
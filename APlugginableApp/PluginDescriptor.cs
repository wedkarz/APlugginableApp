using System;
using System.Collections.Generic;
using System.Text;

namespace APlugginableApp
{
    internal class PluginDescriptor
    {
        private readonly PluginInstanceLifecycle _lifecycle;
        private readonly Type _pluginType;


        private string _pluginInterfaceDescriptionPropertyName = "Description";
        private string _pluginInterfaceExecuteMethodName = "Execute";

        internal PluginDescriptor(Type pluginType, string name, PluginInstanceLifecycle lifecycle)
        {
            _lifecycle = lifecycle;
            _pluginType = pluginType;
            Name = name;
        }

        internal string Name { get; }


        private object _pluginInstance = null;
        private object PluginInstance
        {
            get
            {
                if (_pluginInstance == null || _lifecycle == PluginInstanceLifecycle.AlwaysNew)
                {
                    _pluginInstance = Activator.CreateInstance(_pluginType);
                }

                return _pluginInstance;
            }
        }

        private string _description = null;
        internal string Description
        {
            get
            {
                if (_description == null)
                {
                    var descriptionPropertyInfo = _pluginType.GetProperty(_pluginInterfaceDescriptionPropertyName);
                    _description = (string)descriptionPropertyInfo.GetValue(PluginInstance);
                }

                return _description;
            }
        }

        internal string Execute(string input)
        {
            var methodInfo = _pluginType.GetMethod(_pluginInterfaceExecuteMethodName);
            var pluginObject = Activator.CreateInstance(_pluginType);

            var output = methodInfo.Invoke(pluginObject, new[] { input });
            if (output is string)
            {
                return output as string;
            }

            return string.Empty;
        }
    }
}

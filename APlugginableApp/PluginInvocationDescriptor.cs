using System;
using System.Collections.Generic;
using System.Text;

namespace APlugginableApp
{
    internal class PluginInvocationDescriptor
    {
        internal PluginDescriptor Plugin { get; }
        internal string Argument { get; }

        internal PluginInvocationDescriptor(PluginDescriptor pluginDescriptor, string argument)
        {
            Plugin = pluginDescriptor;
            Argument = argument;
        }
    }
}

using System.Collections.Generic;

namespace APlugginableApp
{
    internal interface IPluginProvider
    {
        IEnumerable<PluginDescriptor> ProvidePlugins();
    }
}
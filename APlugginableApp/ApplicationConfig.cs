using System;
using System.Collections.Generic;
using System.Text;

namespace APlugginableApp
{
    class ApplicationConfig
    {
        PluginInstanceLifecycle PluginsLifecycle { get; set; }
        
        enum PluginInstanceLifecycle
        {
            AlwaysNew,
            SingleForAllCalls
        }
    }
}

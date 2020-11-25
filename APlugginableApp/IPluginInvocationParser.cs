namespace APlugginableApp.CLIv2
{
    internal interface IPluginInvocationParser
    {
        public PluginInvocationDescriptor ParseCommand(string commandString);
    }
}
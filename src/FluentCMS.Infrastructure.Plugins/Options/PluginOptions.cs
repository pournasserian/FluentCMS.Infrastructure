namespace FluentCMS.Infrastructure.Plugins.Options
{
    // Options for configuring the plugin system
    public class PluginOptions
    {
        // Directory where plugin assemblies are stored
        public string PluginsDirectory { get; set; } = "Plugins";
        
        // Whether to automatically enable newly discovered plugins
        public bool AutoEnableNewPlugins { get; set; } = false;
    }
}

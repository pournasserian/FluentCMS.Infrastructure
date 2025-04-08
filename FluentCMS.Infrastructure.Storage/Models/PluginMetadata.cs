using System;

namespace FluentCMS.Infrastructure.Storage.Models
{
    // Database model for plugin metadata
    public class PluginMetadata
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string AssemblyPath { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime InstalledDate { get; set; }
        public DateTime? LastEnabledDate { get; set; }
    }
}

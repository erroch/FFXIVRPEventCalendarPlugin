using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using FFXIVRPCalendarPlugin.Models;

namespace FFXIVRPCalendarPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool UseLocalTimeZone { get; set; } = true;

        public ConfigurationProperties ConfigurationProperties = new ConfigurationProperties();
        
        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}

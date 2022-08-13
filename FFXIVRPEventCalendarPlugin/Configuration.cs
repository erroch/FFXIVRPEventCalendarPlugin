//-----------------------------------------------------------------------
// <copyright file="Configuration.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin
{
    using System;

    using Dalamud.Configuration;
    using Dalamud.Plugin;
    using FFXIVRPCalendarPlugin.Models;

    /// <summary>
    /// Configuration storage for the FFXIV RP Event Calendar plugin.
    /// </summary>
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        /// <summary>
        /// Gets or sets the Version number.
        /// </summary>
        public int Version { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether to use the time zone local to the user for event translations.
        /// </summary>
        public bool UseLocalTimeZone { get; set; } = true;

        /// <summary>
        /// Gets or sets the Calendar specific configuration properties.
        /// </summary>
        public ConfigurationProperties ConfigurationProperties { get; set; } = new ConfigurationProperties();

        /// <summary>
        /// Initialize the Plugin interface.
        /// </summary>
        /// <param name="pluginInterface">The plugin interface provided from the primary Dalamud plugin.</param>
        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        /// <summary>
        /// Trigger the saving of the configuration through the plugin interface.
        /// </summary>
        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}

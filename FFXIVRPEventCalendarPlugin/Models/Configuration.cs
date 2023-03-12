//-----------------------------------------------------------------------
// <copyright file="Configuration.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models
{
    using System;
    using System.Collections.Generic;

    using Dalamud.Configuration;
    using Dalamud.Plugin;

    /// <summary>
    /// Configuration storage for the FFXIV RP Event Calendar plugin.
    /// </summary>
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        private const string DefaultApiURL = "https://api.ffxiv-rp.org/api";

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        /// <summary>
        /// Gets or sets the Version number.
        /// </summary>
        public int Version { get; set; } = 0;

        /// <summary>
        /// Gets or sets the root URL of the event calendar's API.
        /// </summary>
        public string ApiAddress { get; set; } = DefaultApiURL;

        /// <summary>
        /// Gets or sets the time zone to be used when translating event start/end dates and determing the stand and end of 'today'.
        /// </summary>
        public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

        /// <summary>
        /// Gets or sets a listing of ESRB Ratings to filter event lists with.
        /// </summary>
        /// <remarks>
        /// Defaults to "Teen" events only.  Null or empty will show all.
        /// </remarks>
        public List<string> Ratings { get; set; } = new List<string>(new string[] { "Teen" });

        /// <summary>
        /// Gets or sets a listing of categories to filter the event lists with.
        /// </summary>
        public List<string>? Categories { get; set; } = null;

        /// <summary>
        /// Gets or sets the timeframe of events to display.
        /// </summary>
        public EventTimeframe EventTimeframe { get; set; } = EventTimeframe.Today;

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

        /// <summary>
        /// Reset the calendar specific properties to their default values.
        /// </summary>
        public void Reset()
        {
            this.Categories = null;
            this.Ratings = new List<string>(new string[] { "Teen" });
            this.TimeZoneInfo = TimeZoneInfo.Local;
            this.ApiAddress = DefaultApiURL;
        }
    }
}

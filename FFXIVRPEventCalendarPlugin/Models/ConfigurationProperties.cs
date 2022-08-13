//-----------------------------------------------------------------------
// <copyright file="ConfigurationProperties.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents calendar specific configuration properties.
    /// </summary>
    /// <remarks>TODO: Move into the primary configuration class.</remarks>
    [Serializable]
    public class ConfigurationProperties
    {
        /// <summary>
        /// Gets or sets the root URL of the event calendar's API.
        /// </summary>
        public string ApiAddress { get; set; } = "https://api.ffxiv-rp.org/api";

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
        /// Gets or sets a flag indicating the rage of events to display.
        /// </summary>
        [Obsolete("To be removed.")]
        public EventDisplayRange EventDisplayRange { get; set; } = EventDisplayRange.Everywhere;
    }
}

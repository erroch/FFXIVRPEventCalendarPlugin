//-----------------------------------------------------------------------
// <copyright file="TimeZoneTranslationInfo.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendar.Models
{
    /// <summary>
    /// Represents a translation between a TimeZoneInfo name and an IANA name.
    /// </summary>
    public class TimeZoneTranslationInfo
    {
        /// <summary>
        /// Gets or sets the TimeZoneInfo based common time zone name.
        /// </summary>
        public string TimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the IANA standard name for the time zone.
        /// </summary>
        public string IANAName { get; set; }

        /// <summary>
        /// Gets or sets the TimeZoneInfo.Id identifier.
        /// </summary>
        public string MicrosoftTZName { get; set; }
    }
}

//-----------------------------------------------------------------------
// <copyright file="EventTimeframe.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models
{
    using System.ComponentModel;

    /// <summary>
    /// Represents a timeframe of roleplay events to show.
    /// </summary>
    public enum EventTimeframe : int
    {
        /// <summary>
        /// Show only events that are happening right now.
        /// </summary>
        [Description("Happening now")]
        Now = 0,

        /// <summary>
        /// Show only events that will open within the next hour.
        /// </summary>
        [Description("Events opening in the next hour")]
        NextHours = 1,

        /// <summary>
        /// Show only events happening today.
        /// </summary>
        [Description("Today's events")]
        Today = 2,

        /// <summary>
        /// Show only events that are happening this week.
        /// </summary>
        [Description("This week's events")]
        ThisWeek = 3,

        /// <summary>
        /// Show only events that are happening this week.
        /// </summary>
        [Description("Next week's events")]
        NextWeek = 4,
    }
}

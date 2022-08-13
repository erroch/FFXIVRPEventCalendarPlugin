//-----------------------------------------------------------------------
// <copyright file="EventCategoryInfo.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models
{
    /// <summary>
    /// Represents an event category.
    /// </summary>
    public class EventCategoryInfo
    {
        /// <summary>
        /// Gets or sets the event category name.
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the verbose description of the event category.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the event category.
        /// </summary>
        public int SortOrder { get; set; }
    }
}

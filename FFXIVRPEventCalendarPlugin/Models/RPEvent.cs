//-----------------------------------------------------------------------
// <copyright file="RPEvent.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models
{
    using System;

    /// <summary>
    /// Represents a data transfer object containing information about a specific RP Event.
    /// </summary>
    public class RPEvent
    {
        /// <summary>
        /// Gets or sets the Data Center name.
        /// </summary>
        public string? Datacenter { get; set; }

        /// <summary>
        /// Gets or sets the Server name.
        /// </summary>
        public string? Server { get; set; }

        /// <summary>
        /// Gets or sets the Server Identifier.
        /// </summary>
        public uint ServerId { get; set; }

        /// <summary>
        /// Gets or sets the Calendar Event Identifier.
        /// </summary>
        public string? UId { get; set; }

        /// <summary>
        /// Gets or sets the Event Name.
        /// </summary>
        public string? EventName { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the Event URL.
        /// </summary>
        public string? EventURL { get; set; }

        /// <summary>
        /// Gets or sets the ESRB Rating Name.
        /// </summary>
        public string ESRBRating { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Event Description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the Event Category Name.
        /// </summary>
        public string EventCategory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Contact Information.
        /// </summary>
        public string Contacts { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC start time.
        /// </summary>
        public DateTime StartTimeUTC { get; set; }

        /// <summary>
        /// Gets or sets the UTC end time.
        /// </summary>
        public DateTime EndTimeUTC { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the event is recurring.
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Gets or sets the last date the event was validated.
        /// </summary>
        public DateTime? LastValidated { get; set; }

        /// <summary>
        /// Gets or sets the user local start time of the event.
        /// </summary>
        public DateTime LocalStartTime { get; set; }

        /// <summary>
        /// Gets or sets the user local end time of the event.
        /// </summary>
        public DateTime LocalEndTime { get; set; }
    }
}

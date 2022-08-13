//-----------------------------------------------------------------------
// <copyright file="FFXIVServer.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models
{
    /// <summary>
    /// Represents one of FFXIV's servers.
    /// </summary>
    public class FFXIVServer
    {
        /// <summary>
        /// Gets or sets the Server name.
        /// </summary>
        public string? ServerName { get; set; }

        /// <summary>
        /// Gets or sets the Datacenter Name.
        /// </summary>
        public string? DataCenterName { get; set; }

        /// <summary>
        /// Gets or sets the physical datacenter region name.
        /// </summary>
        public string? RegionName { get; set;  }

        /// <summary>
        /// Gets or sets the FFXIV Internal Server Id.
        /// </summary>
        public int ServerId { get; set; }
    }
}

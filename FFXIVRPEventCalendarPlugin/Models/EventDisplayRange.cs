﻿//-----------------------------------------------------------------------
// <copyright file="EventDisplayRange.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models
{
    using System.ComponentModel;

    /// <summary>
    /// Represents the range of servers to show events from.
    /// </summary>
    public enum EventDisplayRange : int
    {
        /// <summary>
        /// Show events from all servers on all datacenters.
        /// </summary>
        [Description("Everywhere")]
        Everywhere = 0,

        /// <summary>
        /// Show events for all servers in the current physical datacenter cluster.
        /// </summary>
        [Description("Physical Datacenter Cluster")]
        PhysicalDatacenter = 1,

        /// <summary>
        /// Show events for all servers in the logical datacenter.
        /// </summary>
        [Description("Local Datacenter")]
        LocalDatacenter = 2,

        /// <summary>
        /// Show events only from the current server.
        /// </summary>
        [Description("Current Server")]
        LocalServer = 3,
    }
}

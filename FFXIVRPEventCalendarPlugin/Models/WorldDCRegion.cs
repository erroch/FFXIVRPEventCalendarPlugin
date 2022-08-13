//-----------------------------------------------------------------------
// <copyright file="WorldDCRegion.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Models
{
    using System.ComponentModel;

    /// <summary>
    /// Represents a World DC DataCenter.
    /// </summary>
    public enum WorldDCRegion : byte
    {
        /// <summary>
        /// Any of the Non-Public Data Centers.
        /// </summary>
        [Description("Non-Public Data Center")]
        NonPublic = 0,

        /// <summary>
        /// The Japanese physical datacenter.
        /// </summary>
        [Description("Japanese Date Center")]
        Japanese = 1,

        /// <summary>
        /// The North American physical data center.
        /// </summary>
        [Description("North American Data Center")]
        NorthAmerica = 2,

        /// <summary>
        /// The European physical data center.
        /// </summary>
        [Description("European Data Center")]
        European = 3,

        /// <summary>
        /// The Oceanian physical data center.
        /// </summary>
        [Description("Oceanian Data Center")]
        Oceanian = 4,
    }
}

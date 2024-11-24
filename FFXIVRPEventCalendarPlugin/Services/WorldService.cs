//-----------------------------------------------------------------------
// <copyright file="WorldService.cs" company="FFXIV RP Event Calendar">
//     Copyright (c) FFXIV RP Event Calendar. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace FFXIVRPCalendarPlugin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FFXIVRPCalendarPlugin;
    using Lumina.Excel;
    using Lumina.Excel.Sheets;

    /// <summary>
    /// Provides world and data center information.
    /// </summary>
    public class WorldService
    {
        private static IDictionary<uint, World>? worldDictionary;
        private static IDictionary<uint, WorldDCGroupType>? datacenterDictionary;

        /// <summary>
        /// Gets a dictionary of World information.
        /// </summary>
        public static IDictionary<uint, World>? Worlds
        {
            get
            {
                if (worldDictionary == null)
                {
                    worldDictionary = BuildWorldDictionary();
                }

                return worldDictionary;
            }
        }

        /// <summary>
        /// Gets a list of World infromation.
        /// </summary>
        public static List<World> WorldList
        {
            get
            {
                if (Worlds == null)
                {
                    return new List<World>();
                }

                return Worlds.Values.Where(x => x.IsPublic).ToList();
            }
        }

        /// <summary>
        /// Gets a dictionary of Datacenter information.
        /// </summary>
        public static IDictionary<uint, WorldDCGroupType>? Datacenters
        {
            get
            {
                if (datacenterDictionary == null)
                {
                    datacenterDictionary = BuildDataCenterDictionary();
                }

                return datacenterDictionary;
            }
        }

        /// <summary>
        /// Gets an array of world (server) identifiers for a given datacenter.
        /// </summary>
        /// <param name="datacenterId">The datacenter identifier.</param>
        /// <returns>An array of world identificers for the provided datacenter.</returns>
        public static uint[] GetDatacenterWorldIds(uint datacenterId)
        {
            if (Worlds != null)
            {
                return Worlds.Where(x => x.Value.DataCenter.Value.RowId == datacenterId)
                    .Select(x => x.Key)
                    .ToArray();
            }
            else
            {
                return Array.Empty<uint>();
            }
        }

        /// <summary>
        /// Gets an array of datacenter identifiers for a given phsyical datacenter region.
        /// </summary>
        /// <param name="regionId">The region identifier.</param>
        /// <returns>An array of datacenter identifiers for the provided region.</returns>
        public static uint[] GetRegionDatacenterIds(byte regionId)
        {
            if (Datacenters != null)
            {
                return Datacenters.Where(x => x.Value.Region == regionId)
                    .Select(x => x.Key)
                    .ToArray();
            }
            else
            {
                return Array.Empty<uint>();
            }
        }

        /// <summary>
        /// Gets an array of world identifiers for a given physical datacenter region.
        /// </summary>
        /// <param name="regionId">The region identifier.</param>
        /// <returns>An array of world identifiers for the privded datacenter region.</returns>
        public static uint[] GetRegionWorldIds(byte regionId)
        {
            uint[] datacenterIds = GetRegionDatacenterIds(regionId);

            if (Worlds != null)
            {
                return Worlds.Where(x => datacenterIds.Contains(x.Value.DataCenter.Value.RowId))
                .Select(x => x.Key)
                .ToArray();
            }
            else
            {
                return Array.Empty<uint>();
            }
        }

        private static IDictionary<uint, World>? BuildWorldDictionary()
        {
            ExcelSheet<World>? worldSheet = Plugin.DataManager.GetExcelSheet<World>();
            if (worldSheet == null)
            {
                return null;
            }

            IDictionary<uint, World> result = worldSheet.ToDictionary(row => row.RowId, row => row);

            return result;
        }

        private static IDictionary<uint, WorldDCGroupType>? BuildDataCenterDictionary()
        {
            ExcelSheet<WorldDCGroupType>? dataCenterSheet = Plugin.DataManager.GetExcelSheet<WorldDCGroupType>();

            if (dataCenterSheet == null)
            {
                return null;
            }

            IDictionary<uint, WorldDCGroupType> result = dataCenterSheet.ToDictionary(row => row.RowId, row => row);

            return result;
        }
    }
}

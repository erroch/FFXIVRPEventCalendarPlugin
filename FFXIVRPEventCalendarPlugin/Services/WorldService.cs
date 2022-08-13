namespace FFXIVRPCalendarPlugin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Lumina.Excel;
    using Lumina.Excel.GeneratedSheets;

    using FFXIVRPCalendarPlugin;

    /// <summary>
    /// Provides world and data center information.
    /// </summary>
    public class WorldService
    {
        private static IDictionary<uint, World>? worldDictionary;
        private static IDictionary<uint, WorldDCGroupType>? datacenterDictionary;

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

        public static List<World> WorldList
        {
            get
            {
                if (Worlds == null)
                {
                    return new List<World>();
                }
                else

                return Worlds.Values.Where(x => x.IsPublic).ToList();
            }
        }

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

        public static uint[] GetDatacenterWorldIds(uint datacenterId)
        {
            if (Worlds != null)
            {
                return Worlds.Where(x => x.Value.DataCenter.Row == datacenterId)
                    .Select(x => x.Key)
                    .ToArray();
            }
            else
            {
                return Array.Empty<uint>();
            }
        }

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

        public static uint[] GetRegionWorldIds(byte regionId)
        {
            uint[] datacenterIds = GetRegionDatacenterIds(regionId);

            if (Worlds != null)
            {
                return Worlds.Where(x => datacenterIds.Contains(x.Value.DataCenter.Row))
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
            ExcelSheet<World>? worldSheet = Plugin.Data.GetExcelSheet<World>();
            if (worldSheet == null)
            {
                return null;
            }

            IDictionary<uint, World> result = worldSheet.ToDictionary(row => row.RowId, row => row);

            return result;
        }

        private static IDictionary<uint, WorldDCGroupType>? BuildDataCenterDictionary()
        {
            ExcelSheet<WorldDCGroupType>? dataCenterSheet = Plugin.Data.GetExcelSheet<WorldDCGroupType>();

            if (dataCenterSheet == null)
            {
                return null;
            }

            IDictionary<uint, WorldDCGroupType> result = dataCenterSheet.ToDictionary(row => row.RowId, row => row);

            return result;
        }
    }
}

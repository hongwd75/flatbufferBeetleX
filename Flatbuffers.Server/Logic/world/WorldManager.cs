using Game.Logic.utility;
using Logic.database.table;

namespace Game.Logic.World
{
    public static class WorldManager
    {
        private static readonly ReaderWriterDictionary<ushort, Region> m_regions = new();
        public static IDictionary<ushort, Region> Regions
        {
            get { return m_regions; }
        }
        
        public static Region GetRegion(ushort regionID)
        {
            Region reg;
            if (m_regions.TryGetValue(regionID, out reg))
            {
                return reg;
            }
            throw new KeyNotFoundException($"Region with ID {regionID} not found.");
        }
        
        // 초기화
        public static bool InitRegion(out RegionData[] regionDatas)
        {
            m_regions.Clear();
            
            var regions = new List<RegionData>(512);
            foreach (DBRegions dbRegion in GameServer.Database.SelectAllObjects<DBRegions>())
            {
                var data = new RegionData();
                
                data.Id = dbRegion.RegionID;
                data.Name = dbRegion.Name;
                data.Description = dbRegion.Description;
                data.Ip = dbRegion.IP;
                data.Port = dbRegion.Port;
                data.Expansion = dbRegion.Expansion;
                data.HousingEnabled = dbRegion.HousingEnabled;
                data.DivingEnabled = dbRegion.DivingEnabled;
                data.WaterLevel = dbRegion.WaterLevel;
                data.ClassType = dbRegion.ClassType;
                data.IsFrontier = dbRegion.IsFrontier;
                
                regions.Add(data);
            }
            regions.Sort();
            
            regionDatas = regions.ToArray();
            return true;
        }
    }
}
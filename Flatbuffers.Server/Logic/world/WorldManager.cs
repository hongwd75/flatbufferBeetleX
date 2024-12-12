using Game.Logic.utility;

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
                return reg;
			
            return null;
        }
        
        
        // 초기화
        
    }
}
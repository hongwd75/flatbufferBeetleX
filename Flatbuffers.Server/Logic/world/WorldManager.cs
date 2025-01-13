using System.Reflection;
using Game.Logic.datatable;
using Game.Logic.managers;
using Game.Logic.network;
using Game.Logic.ServerProperties;
using Game.Logic.Utils;
using Game.Logic.World.Timer;
using log4net;
using Logic.database;
using Logic.database.table;

namespace Game.Logic.World
{
    public static class WorldManager
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        #region ==== 정의값 ============================================================================================
        /// <summary>
        /// Ping timeout definition in seconds
        /// </summary>
        public const long PING_TIMEOUT = 360; // 6 min default ping timeout (ticks are 100 nano seconds)
        /// <summary>
        /// Holds the distance which player get experience from a living object
        /// </summary>
        public const int MAX_EXPFORKILL_DISTANCE = 16384;
        /// <summary>
        /// Is the distance a whisper can be heard
        /// </summary>
        public const int WHISPER_DISTANCE = 512; // tested
        /// <summary>
        /// Is the distance a say is broadcast
        /// </summary>
        public const int SAY_DISTANCE = 512; // tested
        /// <summary>
        /// Is the distance info messages are broadcast (player attacks, spell cast, player stunned/rooted/mezzed, loot dropped)
        /// </summary>
        public const int INFO_DISTANCE = 512; // tested for player attacks, think it's same for rest
        /// <summary>
        /// Is the distance a death message is broadcast when player dies
        /// </summary>
        public const ushort DEATH_MESSAGE_DISTANCE = ushort.MaxValue; // unknown
        /// <summary>
        /// Is the distance a yell is broadcast
        /// </summary>
        public const int YELL_DISTANCE = 1024; // tested
        /// <summary>
        /// Is the distance at which livings can give a item
        /// </summary>
        public const int GIVE_ITEM_DISTANCE = 128;  // tested
        /// <summary>
        /// Is the distance at which livings can interact
        /// </summary>
        public const int INTERACT_DISTANCE = 192;  // tested
        /// <summary>
        /// Is the distance an player can see
        /// </summary>
        public const int VISIBILITY_DISTANCE = 3600;
        /// <summary>
        /// Moving greater than this distance requires the player to do a full world refresh
        /// </summary>
        public const int REFRESH_DISTANCE = 1000;
        /// <summary>
        /// Is the square distance a player can see
        /// </summary>
        public const int VISIBILITY_SQUARE_DISTANCE = 12960000;
        /// <summary>
        /// Holds the distance at which objects are updated
        /// </summary>
        public const int OBJ_UPDATE_DISTANCE = 4096;       
        /// <summary>
        /// This constant defines the day constant
        /// </summary>
        private const int DAY = 86400000;        
        #endregion


        #region ==== 변수들 ============================================================================================
        private static Thread m_relocationThread;
        private static Thread m_WorldUpdateThread;
        private static GameTimer.TimeManager[] m_regionTimeManagers;

        private static int m_dayStartTick;
        private static uint m_dayIncrement;
        private static System.Threading.Timer m_dayResetTimer;    
        #endregion
        
        #region ==== 지역 / 영역 데이터 =================================================================================
        private static readonly ReaderWriterDictionary<ushort, Region> m_regions = new();
        public static IDictionary<ushort, Region> Regions
        {
            get { return m_regions; }
        }
        
        private static readonly ReaderWriterDictionary<ushort, Zone> m_zones = new();

        public static IDictionary<ushort, Zone> Zones
        {
            get { return m_zones; }
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
        
        private static Dictionary<ushort, RegionData> m_regionData;

        public static IDictionary<ushort, RegionData> RegionData
        {
            get { return m_regionData; }
        }

        private static Dictionary<ushort, List<ZoneData>> m_zonesData;
        public static Dictionary<ushort, List<ZoneData>> ZonesData
        {
            get { return m_zonesData; }
        }        
        #endregion
        
        public static GameTimer.TimeManager[] GetRegionTimeManagers()
        {
            GameTimer.TimeManager[] timers = m_regionTimeManagers;
            if (timers == null) return Array.Empty<GameTimer.TimeManager>();
            return (GameTimer.TimeManager[])timers.Clone();
        }

        public static bool Init(RegionData[] regionsData)
        {
            try
            {
                long mobs = 0;
                long merchants = 0;
                long items = 0;
                long bindpoints = 0;
                regionsData.AsParallel().WithDegreeOfParallelism(GameServer.Instance.Configuration.CPUUse << 2).ForAll(data => {
                    Region reg;
                    if (m_regions.TryGetValue(data.Id, out reg))
                        reg.LoadFromDatabase(data.Mobs, ref mobs, ref merchants, ref items, ref bindpoints);
                });
                
                m_WorldUpdateThread = new Thread(new ThreadStart(WorldUpdateThread.WorldUpdateThreadStart));
                m_WorldUpdateThread.Priority = ThreadPriority.AboveNormal;
                m_WorldUpdateThread.Name = "NpcUpdate";
                m_WorldUpdateThread.IsBackground = true;
                m_WorldUpdateThread.Start();

                m_dayIncrement = Math.Max(0, Math.Min(1000, ServerProperties.Properties.WORLD_DAY_INCREMENT)); // increments > 1000 do not render smoothly on clients
                m_dayStartTick = Environment.TickCount - (int)(DAY / Math.Max(1, m_dayIncrement) / 2); // set start time to 12pm
                m_dayResetTimer = new System.Threading.Timer(new TimerCallback(DayReset), null, DAY / Math.Max(1, m_dayIncrement) / 2, DAY / Math.Max(1, m_dayIncrement));
                
                m_relocationThread = new Thread(new ThreadStart(RelocateRegions));
                m_relocationThread.Name = "RelocateReg";
                m_relocationThread.IsBackground = true;
                m_relocationThread.Start();                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return true;
        }

        // 초기화
        public static bool EarlyInit(out RegionData[] regionDatas)
        {
            m_regions.Clear();
            m_zones.Clear();
            m_regionData = new Dictionary<ushort, RegionData>();
            m_zonesData = new Dictionary<ushort, List<ZoneData>>();
            
            log.Debug("loading mobs from DB...");
            
            var mobList = new List<Mob>();
            var disabledRegions = Util.SplitCSV(Properties.DISABLED_REGIONS, true);
            var whereClause = DB.Column(nameof(Mob.Region)).IsNotIn(disabledRegions);
            mobList.AddRange(GameDB<Mob>.SelectObjects(whereClause));            
            
            var mobsByRegionId = new Dictionary<ushort, List<Mob>>(512);
            foreach (Mob mob in mobList)
            {
                List<Mob> list;

                if (!mobsByRegionId.TryGetValue(mob.Region, out list))
                {
                    list = new List<Mob>(1024);
                    mobsByRegionId.Add(mob.Region, list);
                }

                list.Add(mob);
            }
            
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

                List<Mob> mobs;

                if (!mobsByRegionId.TryGetValue(data.Id, out mobs))
                    data.Mobs = Array.Empty<Mob>();
                else
                    data.Mobs = mobs.ToArray();
                
                regions.Add(data);
            }
            
            regions.Sort();
            
            int cpuCount = GameServer.Instance.Configuration.CPUUse;
            if (cpuCount < 1)
            {
                cpuCount = 1;
            }
            
            GameTimer.TimeManager[] timers = new GameTimer.TimeManager[cpuCount];
            for (int i = 0; i < cpuCount; i++)
            {
                timers[i] = new GameTimer.TimeManager(string.Format("RegionTime{0}", (i + 1).ToString()));
            }
            
            m_regionTimeManagers = timers;

            for (int i = 0; i < regions.Count; i++)
            {
                var region = regions[i];
                RegisterRegion(timers[FastMath.Abs(i % (cpuCount * 2) - cpuCount) % cpuCount], region);
            }            
            
            
            foreach (Zones dbZone in GameServer.Database.SelectAllObjects<Zones>())
            {
                ZoneData zoneData = new ZoneData();
                zoneData.Height = (byte)dbZone.Height;
                zoneData.Width = (byte)dbZone.Width;
                zoneData.OffY = (byte)dbZone.OffsetY;
                zoneData.OffX = (byte)dbZone.OffsetX;
                zoneData.Description = dbZone.Name;
                zoneData.RegionID = dbZone.RegionID;
                zoneData.ZoneID = (ushort)dbZone.ZoneID;
                zoneData.WaterLevel = dbZone.WaterLevel;
                zoneData.DivingFlag = dbZone.DivingFlag;
                zoneData.IsLava = dbZone.IsLava;
                RegisterZone(zoneData, zoneData.ZoneID, zoneData.RegionID, zoneData.Description,
                    dbZone.Experience, dbZone.Realmpoints, dbZone.Bountypoints, dbZone.Coin, dbZone.Realm);

                //Save the zonedata.
                if (!m_zonesData.ContainsKey(zoneData.RegionID))
                    m_zonesData.Add(zoneData.RegionID, new List<ZoneData>());

                m_zonesData[zoneData.RegionID].Add(zoneData);
            }            
            //-----------------------------------------------------------------------------------------
            regionDatas = regions.ToArray();
            return true;
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        public static Region RegisterRegion(GameTimer.TimeManager time, RegionData data)
        {
            Region region =  Region.Create(time, data);
            m_regions.Add(data.Id, region);
            return region;
        }
        
        public static void RegisterZone(ZoneData zoneData, ushort zoneID, ushort regionID, string zoneName, int xpBonus, int rpBonus, int bpBonus, int coinBonus, byte realm)
        {
            Region region = GetRegion(regionID);
            if (region == null)
            {
                if (log.IsWarnEnabled)
                {
                    log.WarnFormat("Could not find Region {0} for Zone {1}", regionID, zoneData.Description);
                }
                return;
            }
			
            // Making an assumption that a zone waterlevel of 0 means it is not set and we should use the regions waterlevel - Tolakram
            if (zoneData.WaterLevel == 0)
            {
                zoneData.WaterLevel = region.WaterLevel;
            }

            bool isDivingEnabled = region.IsRegionDivingEnabled;

            if (zoneData.DivingFlag == 1)
                isDivingEnabled = true;
            else if (zoneData.DivingFlag == 2)
                isDivingEnabled = false;
			
            Zone zone = new Zone(region,
                zoneID,
                zoneName,
                zoneData.OffX * 8192,
                zoneData.OffY * 8192,
                zoneData.Width * 8192,
                zoneData.Height * 8192,
                zoneData.ZoneID,
                isDivingEnabled,
                zoneData.WaterLevel,
                zoneData.IsLava,
                xpBonus,
                rpBonus,
                bpBonus,
                coinBonus,
                realm);

            region.Zones.Add(zone);
            m_zones.AddOrReplace(zoneID, zone);
            log.InfoFormat("   - Added a zone, {0}, to region {1}", zoneData.Description, region.Name);
        }

        private static void RelocateRegions()
        {
            log.InfoFormat("started RelocateRegions() thread ID:{0}", Thread.CurrentThread.ManagedThreadId);
            while (m_relocationThread != null && m_relocationThread.IsAlive)
            {
                try
                {
                    Thread.Sleep(200); // check every 200ms for needed relocs
                    int start = Environment.TickCount;

                    var regionsClone = m_regions.Values;

                    foreach (Region region in regionsClone)
                    {
                        if (region.NumPlayers > 0 && (region.LastRelocationTime + Zone.MAX_REFRESH_INTERVAL) * 10 * 1000 < DateTime.Now.Ticks)
                        {
                            region.Relocate();
                        }
                    }
                    int took = Environment.TickCount - start;
                    if (took > 500)
                    {
                        if (log.IsWarnEnabled)
                            log.WarnFormat("RelocateRegions() took {0}ms", took);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    //On Thread interrupt exit!
                    return;
                }
                catch (Exception e)
                {
                    log.Error(e.ToString());
                }
            }
            log.InfoFormat("# stopped RelocateRegions() thread ID:{0}", Thread.CurrentThread.ManagedThreadId);
        }
        
        private static void DayReset(object sender)
        {
            m_dayStartTick = Environment.TickCount;
            
            foreach (GameClient client in GameServer.Instance.Clients.GetAllPlayingClients())
            {
                if (client.Player != null && client.Player.CurrentRegion != null && client.Player.CurrentRegion.UseTimeManager)
                {
                    client.Out?.SendTime();
                }
            }
        }        
    }
}
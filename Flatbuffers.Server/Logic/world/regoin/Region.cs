﻿using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using Game.Logic.datatable;
using Game.Logic.Events;
using Game.Logic.Geometry;
using Game.Logic.ServerProperties;
using Game.Logic.Utils;
using Game.Logic.World.Timer;
using log4net;
using Logic.database;
using Logic.database.table;

namespace Game.Logic.World;
/*
 *  Rgion
 *   - zone
 *   - zone
 */ 
    public class Region
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Region Variables

        /// <summary>
        /// This is the minimumsize for object array that is allocated when
        /// the first object is added to the region must be dividable by 32 (optimization)
        /// </summary>
        public static readonly int MINIMUMSIZE = 256;


        /// <summary>
        /// This holds all objects inside this region. Their index = their id!
        /// </summary>
        protected GameObject[] m_objects;


        /// <summary>
        /// Object to lock when changing objects in the array
        /// </summary>
        public readonly object ObjectsSyncLock = new object();

        /// <summary>
        /// This holds a counter with the absolute count of all objects that are actually in this region
        /// </summary>
        protected int m_objectsInRegion;

        /// <summary>
        /// Total number of objects in this region
        /// </summary>
        public int TotalNumberOfObjects
        {
            get { return m_objectsInRegion; }
        }

        /// <summary>
        /// This array holds a bitarray
        /// Its used to know which slots in region object array are free and what allocated
        /// This is used to accelerate inserts a lot
        /// </summary>
        protected uint[] m_objectsAllocatedSlots;

        /// <summary>
        /// This holds the index of a possible next object slot
        /// but needs further checks (basically its lastaddedobjectIndex+1)
        /// </summary>
        protected int m_nextObjectSlot;
        

        /// <summary>
        /// Holds all the Zones inside this Region
        /// </summary>
        protected readonly ReaderWriterList<Zone> m_zones;

        protected object m_lockAreas = new object();

        /// <summary>
        /// Holds all the Areas inside this Region
        /// 
        /// ZoneID, AreaID, Area
        ///
        /// Areas can be registed to a reagion via AddArea
        /// and events will be thrown if players/npcs/objects enter leave area
        /// </summary>
        private Dictionary<ushort, IArea> m_Areas;

        protected Dictionary<ushort, IArea> Areas
        {
            get { return m_Areas; }
        }

        /// <summary>
        /// Cache for zone area mapping to quickly access all areas within a certain zone
        /// </summary>
        protected ushort[][] m_ZoneAreas;

        /// <summary>
        /// /// Cache for number of items in m_ZoneAreas array.
        /// </summary>
        protected ushort[] m_ZoneAreasCount;

        /// <summary>
        /// How often shall we remove unused objects
        /// </summary>
        protected static readonly int CLEANUPTIMER = 60000;

        /// <summary>
        /// Contains the # of players in the region
        /// </summary>
        protected int m_numPlayer = 0;

        /// <summary>
        /// last relocation time
        /// </summary>
        private long m_lastRelocationTime = 0;

        /// <summary>
        /// The region time manager
        /// </summary>
        protected readonly GameTimer.TimeManager m_timeManager;
        
        /// <summary>
        /// The Region Mob's Respawn Timer Collection
        /// </summary>
        protected readonly ConcurrentDictionary<GameNPC, int> m_mobsRespawning = new ConcurrentDictionary<GameNPC, int>();

        #endregion

        #region Constructor

        private RegionData m_regionData;
        public RegionData RegionData
        {
            get { return m_regionData; }
            protected set { m_regionData = value; }
        }

        /// <summary>
        /// Factory method to create regions.  Will create a region of data.ClassType, or default to Region if 
        /// an error occurs or ClassType is not specified
        /// </summary>
        /// <param name="time"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Region Create(GameTimer.TimeManager time, RegionData data)
        {
            try
            {
                Type t = typeof(Region);

                if (string.IsNullOrEmpty(data.ClassType) == false)
                {
                    t = Type.GetType(data.ClassType);

                    if (t == null)
                    {
                        t = ScriptMgr.GetType(data.ClassType);
                    }

                    if (t != null)
                    {
                        ConstructorInfo info = t.GetConstructor(new Type[] { typeof(GameTimer.TimeManager), typeof(RegionData) });

                        Region r = (Region)info.Invoke(new object[] { time, data });

                        if (r != null)
                        {
                            // Success with requested classtype
                            log.InfoFormat("Created Region {0} using ClassType '{1}'", r.ID, data.ClassType);
                            return r;
                        }

                        log.ErrorFormat("Failed to Invoke Region {0} using ClassType '{1}'", r.ID, data.ClassType);
                    }
                    else
                    {
                        log.ErrorFormat("Failed to find ClassType '{0}' for region {1}!", data.ClassType, data.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to start region {0} with requested classtype: {1}.  Exception: {2}!", data.Id, data.ClassType, ex.Message);
            }

            // Create region using default type
            return new Region(time, data);
        }

        /// <summary>
        /// Constructs a new empty Region
        /// </summary>
        /// <param name="time">The time manager for this region</param>
        /// <param name="data">The region data</param>
        public Region(GameTimer.TimeManager time, RegionData data)
        {
            m_regionData = data;
            m_objects = Array.Empty<GameObject>();
            m_objectsInRegion = 0;
            m_nextObjectSlot = 0;
            m_objectsAllocatedSlots = Array.Empty<uint>();

            m_zones = new ReaderWriterList<Zone>(1);
            m_ZoneAreas = new ushort[64][];
            m_ZoneAreasCount = new ushort[64];
            for (int i = 0; i < 64; i++)
            {
                m_ZoneAreas[i] = new ushort[AbstractArea.MAX_AREAS_PER_ZONE];
            }

            m_Areas = new Dictionary<ushort, IArea>();

            m_timeManager = time;

            List<string> list = null;

            if (list != null && list.Count > 0)
            {
                m_loadObjects = false;

                foreach (string region in list)
                {
                    if (region.ToString() == ID.ToString())
                    {
                        m_loadObjects = true;
                        break;
                    }
                }
            }

            list = Util.SplitCSV(ServerProperties.Properties.DISABLED_REGIONS, true);
            foreach (string region in list)
            {
                if (region.ToString() == ID.ToString())
                {
                    m_isDisabled = true;
                    break;
                }
            }

            list = Util.SplitCSV(ServerProperties.Properties.DISABLED_EXPANSIONS, true);
            foreach (string expansion in list)
            {
                if (expansion.ToString() == m_regionData.Expansion.ToString())
                {
                    m_isDisabled = true;
                    break;
                }
            }
        }



        /// <summary>
        /// What to do when the region collapses.
        /// This is called when instanced regions need to be closed
        /// </summary>
        public virtual void OnCollapse()
        {
            //Delete objects
            foreach (GameObject obj in m_objects)
            {
                if (obj != null)
                {
                    obj.Delete();
                    RemoveObject(obj);
                    obj.CurrentRegion = null;
                }
            }

            m_objects = null;

            foreach (Zone z in m_zones)
            {
                z.Delete();
            }

            m_zones.Clear();

            GameEventManager.RemoveAllHandlersForObject(this);
        }


        #endregion

        /// <summary>
        /// Handles players leaving this region via a zonepoint
        /// </summary>
        /// <param name="player"></param>
        /// <param name="zonePoint"></param>
        /// <returns>false to halt processing of this request</returns>
        public virtual bool OnZonePoint(GamePlayer player, ZonePoint zonePoint)
        {
            return true;
        }

        #region Properties

        public virtual bool IsRvR
        {
            get
            {
                switch (m_regionData.Id)
                {
                    case 163://new frontiers
                    case 165://cathal valley
                    case 233://Sumoner hall
                    case 234://1to4BG
                    case 235://5to9BG
                    case 236://10to14BG
                    case 237://15to19BG
                    case 238://20to24BG
                    case 239://25to29BG
                    case 240://30to34BG
                    case 241://35to39BG
                    case 242://40to44BG and Test BG
                    case 244://Frontiers RvR dungeon
                    case 249://Darkness Falls - RvR dungeon
                    case 489://lvl5-9 Demons breach
                        return true;
                    default:
                        return false;
                }
            }
        }

        public virtual bool IsFrontier
        {
            get { return m_regionData.IsFrontier; }
            set { m_regionData.IsFrontier = value; }
        }

        /// <summary>
        /// Is the Region a temporary instance
        /// </summary>
        public virtual bool IsInstance
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Is this region a standard DAoC region or a custom server region
        /// </summary>
        public virtual bool IsCustom
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether this region is a dungeon or not
        /// </summary>
        public virtual bool IsDungeon
        {
            get
            {
                const int dungeonOffset = 8192;
                const int zoneCount = 1;

                if (Zones.Count != zoneCount)
                    return false; //Dungeons only have 1 zone!

                var zone = Zones[0];

                if (zone.Offset.X == dungeonOffset && zone.Offset.Y == dungeonOffset)
                    return true; //Only dungeons got this offset

                return false;
            }
        }

        /// <summary>
        /// Gets the # of players in the region
        /// </summary>
        public virtual int NumPlayers
        {
            get { return m_numPlayer; }
        }

        /// <summary>
        /// The Region Name eg. Region000
        /// </summary>
        public virtual string Name
        {
            get { return m_regionData.Name; }
        }
        //Dinberg: Changed this to virtual, so that Instances can take a unique Name, for things like quest instances.

        /// <summary>
        /// The Regi on Description eg. Cursed Forest
        /// </summary>
        public virtual string Description
        {
            get { return m_regionData.Description; }
        }
        //Dinberg: Virtual, so that we can change this if need be, for quests eg 'Hermit Dinbargs Cave'
        //or for the hell of it, eg Jordheim (Instance).

        /// <summary>
        /// The ID of the Region eg. 21
        /// </summary>
        public virtual ushort ID
        {
            get { return m_regionData.Id; }
        }
        //Dinberg: Changed this to virtual, so that Instances can take a unique ID.

        /// <summary>
        /// The Region Server IP ... for future use
        /// </summary>
        public string ServerIP
        {
            get { return m_regionData.Ip; }
        }

        /// <summary>
        /// The Region Server Port ... for future use
        /// </summary>
        public ushort ServerPort
        {
            get { return m_regionData.Port; }
        }

        /// <summary>
        /// An ArrayList of all Zones within this Region
        /// </summary>
        public IList<Zone> Zones
        {
            get { return m_zones; }
        }

        /// <summary>
        /// Returns the object array of this region
        /// </summary>
        public GameObject[] Objects
        {
            get { return m_objects; }
        }

        /// <summary>
        /// Gets or Sets the region expansion (we use client expansion + 1)
        /// </summary>
        public virtual int Expansion
        {
            get { return m_regionData.Expansion + 1; }
        }

        /// <summary>
        /// Gets or Sets the water level in this region
        /// </summary>
        public virtual int WaterLevel
        {
            get { return m_regionData.WaterLevel; }
        }

        /// <summary>
        /// Gets or Sets diving flag for region
        /// Note: This flag should normally be checked at the zone level
        /// </summary>
        public virtual bool IsRegionDivingEnabled
        {
            get { return m_regionData.DivingEnabled; }
        }

        /// <summary>
        /// Does this region contain housing?
        /// </summary>
        public virtual bool HousingEnabled
        {
            get { return m_regionData.HousingEnabled; }
        }

        /// <summary>
        /// Should this region use the housing manager?
        /// Standard regions always use the housing manager if housing is enabled, custom regions might not.
        /// </summary>
        public virtual bool UseHousingManager
        {
            get { return HousingEnabled; }
        }

        /// <summary>
        /// Gets last relocation time
        /// </summary>
        public long LastRelocationTime
        {
            get { return m_lastRelocationTime; }
        }

        /// <summary>
        /// Gets the region time manager
        /// </summary>
        public virtual GameTimer.TimeManager TimeManager
        {
            get { return m_timeManager; }
        }

        /// <summary>
        /// Gets the current region time in milliseconds
        /// </summary>
        public virtual long Time
        {
            get { return m_timeManager.CurrentTime; }
        }

        protected bool m_isDisabled = false;
        /// <summary>
        /// Is this region disabled
        /// </summary>
        public virtual bool IsDisabled
        {
            get { return m_isDisabled; }
        }

        protected bool m_loadObjects = true;
        /// <summary>
        /// Will this region load objects
        /// </summary>
        public virtual bool LoadObjects
        {
            get { return m_loadObjects; }
        }
        
        public virtual ushort Skin
        {
            get { return ID; }
        }

        public virtual bool UseTimeManager
        {
            get { return true; }
            set { }
        }
        
        public virtual uint GameTime
        {
            get { return WorldManager.GetCurrentGameTime(); }
            set { }
        }
        
        public virtual uint DayIncrement
        {
            get { return WorldManager.GetDayIncrement(); }
            set { }
        }
        
        public virtual bool IsAM
        {
            get
            {
                if (IsPM)
                    return false;
                return true;
            }
        }

        private bool m_isPM;
        /// <summary>
        /// Determine if the current time is PM.
        /// </summary>
        public virtual bool IsPM
        {
            get
            {
                uint cTime = GameTime;

                uint hour = cTime / 1000 / 60 / 60;
                bool pm = false;

                if (hour == 0)
                {
                    hour = 12;
                }
                else if (hour == 12)
                {
                    pm = true;
                }
                else if (hour > 12)
                {
                    hour -= 12;
                    pm = true;
                }
                m_isPM = pm;

                return m_isPM;
            }
            set { m_isPM = value; }
        }

        private bool m_isNightTime;
        /// <summary>
        /// Determine if current time is between 6PM and 6AM, can be used for conditional spells.
        /// </summary>
        public virtual bool IsNightTime
        {
            get
            {
                uint cTime = GameTime;

                uint hour = cTime / 1000 / 60 / 60;
                bool pm = false;

                if (hour == 0)
                {
                    hour = 12;
                }
                else if (hour == 12)
                {
                    pm = true;
                }
                else if (hour > 12)
                {
                    hour -= 12;
                    pm = true;
                }

                if (pm && hour >= 6)
                    m_isNightTime = true;

                if (!pm && hour <= 5)
                    m_isNightTime = true;

                if (!pm && hour == 12) //Special Handling for Midnight.
                    m_isNightTime = true;

                if (!pm && hour >= 6)
                    m_isNightTime = false;

                if (pm && hour < 6)
                    m_isNightTime = false;

                return m_isNightTime;
            }
            set { m_isNightTime = value; }
        }

        public virtual ConcurrentDictionary<GameNPC, int> MobsRespawning
        {
        	get
        	{
        		return m_mobsRespawning;
        	}
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Starts the RegionMgr
        /// </summary>
        public void StartRegionMgr()
        {
            m_timeManager.Start();
            this.Notify(RegionEvent.RegionStart, this);
        }

        /// <summary>
        /// Stops the RegionMgr
        /// </summary>
        public void StopRegionMgr()
        {
            m_timeManager.Stop();
            this.Notify(RegionEvent.RegionStop, this);
        }

        /// <summary>
        /// Reallocates objects array with given size
        /// </summary>
        /// <param name="count">The size of new objects array, limited by MAXOBJECTS</param>
        public virtual void PreAllocateRegionSpace(int count)
        {
            if (count > Properties.REGION_MAX_OBJECTS)
                count = Properties.REGION_MAX_OBJECTS;
            lock (ObjectsSyncLock)
            {
                if (m_objects.Length > count) return;
                GameObject[] newObj = new GameObject[count];
                Array.Copy(m_objects, newObj, m_objects.Length);
                if (count / 32 + 1 > m_objectsAllocatedSlots.Length)
                {
                    uint[] slotarray = new uint[count / 32 + 1];
                    Array.Copy(m_objectsAllocatedSlots, slotarray, m_objectsAllocatedSlots.Length);
                    m_objectsAllocatedSlots = slotarray;
                }
                m_objects = newObj;
            }
        }

        /// <summary>
        /// Loads the region from database
        /// </summary>
        /// <param name="mobObjs"></param>
        /// <param name="mobCount"></param>
        /// <param name="merchantCount"></param>
        /// <param name="itemCount"></param>
        /// <param name="bindCount"></param>
        public virtual void LoadFromDatabase(Mob[] mobObjs, ref long mobCount, ref long merchantCount, ref long itemCount, ref long bindCount)
        {
            if (!LoadObjects)
                return;

            Assembly gasm = Assembly.GetAssembly(typeof(GameServer));
            var staticObjs = GameDB<WorldObject>.SelectObjects(DB.Column(nameof(WorldObject.Region)).IsEqualTo(ID));
            var bindPoints = GameDB<BindPoint>.SelectObjects(DB.Column(nameof(BindPoint.Region)).IsEqualTo(ID));
            int count = mobObjs.Length + staticObjs.Count;
            if (count > 0) PreAllocateRegionSpace(count + 100);
            int myItemCount = staticObjs.Count;
            int myMobCount = 0;
            int myBindCount = bindPoints.Count;
            string allErrors = string.Empty;

            if (mobObjs.Length > 0)
            {
                foreach (Mob mob in mobObjs)
                {
                    GameNPC myMob = null;
                    string error = string.Empty;
  
                    // Default Classtype
                    string classtype = ServerProperties.Properties.GAMENPC_DEFAULT_CLASSTYPE;
                    
                    // load template if any
                    INpcTemplate template = null;
                    if(mob.NPCTemplateID != -1)
                    {
                    	template = NpcTemplateMgr.GetTemplate(mob.NPCTemplateID);
                    }
                    

                    if ( mob.Guild.Length > 0 && mob.Realm >= 0 && mob.Realm <= (int)eRealm._Last)
                    {
                        Type type = ScriptMgr.FindNPCGuildScriptClass(mob.Guild, (eRealm)mob.Realm);
                        if (type != null)
                        {
                            try
                            {
                                
                                myMob = (GameNPC)type.Assembly.CreateInstance(type.FullName);
                               	
                            }
                            catch (Exception e)
                            {
                                if (log.IsErrorEnabled)
                                    log.Error("LoadFromDatabase", e);
                            }
                        }
                    }

  
                    if (myMob == null)
                    {
                    	if(template != null && template.ClassType != null && template.ClassType.Length > 0 && template.ClassType != Mob.DEFAULT_NPC_CLASSTYPE && template.ReplaceMobValues)
                    	{
                			classtype = template.ClassType;
                    	}
                        else if (mob.ClassType != null && mob.ClassType.Length > 0 && mob.ClassType != Mob.DEFAULT_NPC_CLASSTYPE)
                        {
                            classtype = mob.ClassType;
                        }

                        try
                        {
                            myMob = (GameNPC)gasm.CreateInstance(classtype, false);
                        }
                        catch
                        {
                            error = classtype;
                        }

                        if (myMob == null)
                        {
                            foreach (Assembly asm in ScriptMgr.Scripts)
                            {
                                try
                                {
                                    myMob = (GameNPC)asm.CreateInstance(classtype, false);
                                    error = string.Empty;
                                }
                                catch
                                {
                                    error = classtype;
                                }

                                if (myMob != null)
                                    break;
                            }

                            if (myMob == null)
                            {
                                myMob = new GameNPC();
                                error = classtype;
                            }
                        }
                    }

                    if (!allErrors.Contains(error))
                        allErrors += " " + error + ",";

                    if (myMob != null)
                    {
                        try
                        {
                            myMob.LoadFromDatabase(mob);
                            myMobCount++;
                        }
                        catch (Exception e)
                        {
                            if (log.IsErrorEnabled)
                                log.Error("Failed: " + myMob.GetType().FullName + ":LoadFromDatabase(" + mob.GetType().FullName + ");", e);
                            throw;
                        }

                        myMob.AddToWorld();
                    }
                }
            }

            if (staticObjs.Count > 0)
            {
                foreach (WorldObject item in staticObjs)
                {
                    GameStaticItem myItem;
                    if (!string.IsNullOrEmpty(item.ClassType))
                    {
                        myItem = gasm.CreateInstance(item.ClassType, false) as GameStaticItem;
                        if (myItem == null)
                        {
                            foreach (Assembly asm in ScriptMgr.Scripts)
                            {
                                try
                                {
                                    myItem = (GameStaticItem)asm.CreateInstance(item.ClassType, false);
                                }
                                catch { }
                                if (myItem != null)
                                    break;
                            }
                            if (myItem == null)
                                myItem = new GameStaticItem();
                        }
                    }
                    else
                        myItem = new GameStaticItem();

                    myItem.LoadFromDatabase(item);
                    myItem.AddToWorld();
                    //						if (!myItem.AddToWorld())
                    //							log.ErrorFormat("Failed to add the item to the world: {0}", myItem.ToString());
                }
            }

            foreach (BindPoint point in bindPoints)
            {
                AddArea(new Area.BindArea("bind point", point));
            }

            if (myMobCount + myItemCount + myBindCount > 0)
            {
                if (log.IsInfoEnabled)
                    log.Info(String.Format("Region: {0} ({1}) loaded {2} mobs, {3} items {4} bindpoints, from DB ({5})", Description, ID, myMobCount, myItemCount, myBindCount, TimeManager.Name));

                log.Debug("Used Memory: " + GC.GetTotalMemory(false) / 1024 / 1024 + "MB");

                if (allErrors != string.Empty)
                    log.Error("Error loading the following NPC ClassType(s), GameNPC used instead:" + allErrors.TrimEnd(','));

                Thread.Sleep(0);  // give up remaining thread time to other resources
            }
            Interlocked.Add(ref mobCount, myMobCount);
            Interlocked.Add(ref itemCount, myItemCount);
            Interlocked.Add(ref bindCount, myBindCount);
        }

        /// <summary>
        /// Adds an object to the region and assigns the object an id
        /// </summary>
        /// <param name="obj">A GameObject to be added to the region</param>
        /// <returns>success</returns>
        internal bool AddObject(GameObject obj)
        {
            //Thread.Sleep(10000);
            Zone zone = GetZone(obj.Coordinate);
            if (zone == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("Zone not found for Object: " + obj.Name + "(ID=" + obj.InternalID + ")");
            }

            //Assign a new id
            lock (ObjectsSyncLock)
            {
                if (obj.ObjectID != -1)
                {
                    if (obj.ObjectID < m_objects.Length && obj == m_objects[obj.ObjectID - 1])
                    {
                        log.WarnFormat("Object is already in the region ({0})", obj.ToString());
                        return false;
                    }
                    log.Warn(obj.Name + " should be added to " + Description + " but had already an OID(" + obj.ObjectID + ") => not added\n" + Environment.StackTrace);
                    return false;
                }

                GameObject[] objectsRef = m_objects;

                //*** optimized object management for memory saving primary but keeping it very fast - Blue ***

                // find first free slot for the object
                int objID = m_nextObjectSlot;
                if (objID >= m_objects.Length || m_objects[objID] != null)
                {

                    // we are at array end, are there any holes left?
                    if (m_objects.Length > m_objectsInRegion)
                    {
                        // yes there are some places left in current object array, try to find them
                        // by using the bit array (can check 32 slots at once!)

                        int i = m_objects.Length / 32;
                        // INVARIANT: i * 32 is always lower or equal to m_objects.Length (integer division property)
                        if (i * 32 == m_objects.Length)
                        {
                            i -= 1;
                        }

                        bool found = false;
                        objID = -1;

                        while (!found && (i >= 0))
                        {
                            if (m_objectsAllocatedSlots[i] != 0xffffffff)
                            {
                                // we found a free slot
                                // => search for exact place

                                int currentIndex = i * 32;
                                int upperBound = (i + 1) * 32;
                                while (!found && (currentIndex < m_objects.Length) && (currentIndex < upperBound))
                                {
                                    if (m_objects[currentIndex] == null)
                                    {
                                        found = true;
                                        objID = currentIndex;
                                    }

                                    currentIndex++;
                                }

                                // INVARIANT: at this point, found must be true (otherwise the underlying data structure is corrupt)
                            }

                            i--;
                        }
                    }
                    else
                    { // our array is full, we must resize now to fit new objects

                        if (objectsRef.Length == 0)
                        {

                            // there is no array yet, so set it to a minimum at least
                            objectsRef = new GameObject[MINIMUMSIZE];
                            Array.Copy(m_objects, objectsRef, m_objects.Length);
                            objID = 0;

                        }
                        else if (objectsRef.Length >= Properties.REGION_MAX_OBJECTS)
                        {

                            // no available slot
                            if (log.IsErrorEnabled)
                                log.Error("Can't add new object - region '" + Description + "' is full. (object: " + obj.ToString() + ")");
                            return false;

                        }
                        else
                        {

                            // we need to add a certain amount to grow
                            int size = (int)(m_objects.Length * 1.20);
                            if (size < m_objects.Length + 256)
                                size = m_objects.Length + 256;
                            if (size > Properties.REGION_MAX_OBJECTS)
                                size = Properties.REGION_MAX_OBJECTS;
                            objectsRef = new GameObject[size]; // grow the array by 20%, at least 256
                            Array.Copy(m_objects, objectsRef, m_objects.Length);
                            objID = m_objects.Length; // new object adds right behind the last object in old array

                        }
                        // resize the bitarray as well
                        int diff = objectsRef.Length / 32 - m_objectsAllocatedSlots.Length;
                        if (diff >= 0)
                        {
                            uint[] newBitArray = new uint[Math.Max(m_objectsAllocatedSlots.Length + diff + 50, 100)];	// add at least 100 integers, makes it resize less often, serves 3200 new objects, only 400 bytes
                            Array.Copy(m_objectsAllocatedSlots, newBitArray, m_objectsAllocatedSlots.Length);
                            m_objectsAllocatedSlots = newBitArray;
                        }
                    }
                }

                if (objID < 0)
                {
                    log.Warn("There was an unexpected problem while adding " + obj.Name + " to " + Description);
                    return false;
                }

                // if we found a slot add the object
                GameObject oidObj = objectsRef[objID];
                if (oidObj == null)
                {
                    objectsRef[objID] = obj;
                    m_nextObjectSlot = objID + 1;
                    m_objectsInRegion++;
                    obj.ObjectID = objID + 1;
                    m_objectsAllocatedSlots[objID / 32] |= (uint)1 << (objID % 32);
                    Thread.MemoryBarrier();
                    m_objects = objectsRef;

                    if (obj is GamePlayer)
                    {
                        ++m_numPlayer;
                    }
                    return true;
                }
                else
                {
                    // no available slot
                    if (log.IsErrorEnabled)
                        log.Error("Can't add new object - region '" + Description + "' (object: " + obj.ToString() + "); OID is used by " + oidObj.ToString());
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes the object with the specified ID from the region
        /// </summary>
        /// <param name="obj">A GameObject to be removed from the region</param>
        internal void RemoveObject(GameObject obj)
        {
            lock (ObjectsSyncLock)
            {
                int index = obj.ObjectID - 1;
                if (index < 0)
                {
                    return;
                }

                if (obj is GamePlayer)
                {
                    --m_numPlayer;
                }

                GameObject inPlace = m_objects[obj.ObjectID - 1];
                if (inPlace == null)
                {
                    log.Error("RemoveObject conflict! OID" + obj.ObjectID + " " + obj.Name + "(" + obj.CurrentRegionID + ") but there was no object at that slot");
                    log.Error(new StackTrace().ToString());
                    return;
                }
                if (obj != inPlace)
                {
                    log.Error("RemoveObject conflict! OID" + obj.ObjectID + " " + obj.Name + "(" + obj.CurrentRegionID + ") but there was another object already " + inPlace.Name + " region:" + inPlace.CurrentRegionID + " state:" + inPlace.ObjectState);
                    log.Error(new StackTrace().ToString());
                    return;
                }

                if (m_objects[index] != obj)
                {
                    log.Error("Object OID is already used by another object! (used by:" + m_objects[index].ToString() + ")");
                }
                else
                {
                    m_objects[index] = null;
                    m_nextObjectSlot = index;
                    m_objectsAllocatedSlots[index / 32] &= ~(uint)(1 << (index % 32));
                }
                obj.ObjectID = -1; // invalidate object id
                m_objectsInRegion--;
            }
        }

        /// <summary>
        /// Gets the object with the specified ID
        /// </summary>
        /// <param name="id">The ID of the object to get</param>
        /// <returns>The object with the specified ID, null if it didn't exist</returns>
        public GameObject GetObject(ushort id)
        {
            if (m_objects == null || id <= 0 || id > m_objects.Length)
                return null;
            return m_objects[id - 1];
        }

        public Zone GetZone(Coordinate coordinate)
        {
            foreach (var zone in m_zones)
            {
                var isInZone = zone.Offset.X <= coordinate.X && zone.Offset.Y <= coordinate.Y 
                    && (zone.Offset.X + zone.Width) > coordinate.X && (zone.Offset.Y + zone.Height) > coordinate.Y;
                if (isInZone) return zone;
            }
            return null;
        }

        [Obsolete("Use .GetZone(Coordinate) instead!")]
        public Zone GetZone(int x, int y)
            => GetZone(Coordinate.Create(x,y));

        [Obsolete("Use Zone.XOffset calculation instead.")]
        public int GetXOffInZone(int x, int y)
        {
            Zone z = GetZone(Coordinate.Create(x, y));
            if (z == null)
                return 0;
            return x - z.Offset.X;
        }

        [Obsolete("Use Zone.YOffset calculation instead.")]
        public int GetYOffInZone(int x, int y)
        {
            Zone z = GetZone(Coordinate.Create(x, y));
            if (z == null)
                return 0;
            return y - z.Offset.Y;
        }

        /// <summary>
        /// Check if this region is a capital city
        /// </summary>
        /// <returns>True, if region is a capital city, else false</returns>
        public virtual bool IsCapitalCity
        {
            get
            {
                switch (this.Skin)
                {
                    case 10: return true; // Camelot City
                    case 101: return true; // Jordheim
                    case 201: return true; // Tir na Nog
                    default: return false;
                }
            }
        }

        /// <summary>
        /// Check if this region is a housing zone
        /// </summary>
        /// <returns>True, if region is a housing zone, else false</returns>
        public virtual bool IsHousing
        {
            get
            {
                switch (this.Skin) // use the skin of the region
                {
                    case 2: return true; 	// Housing alb
                    case 102: return true; 	// Housing mid
                    case 202: return true; 	// Housing hib
                    default: return false;
                }
            }
        }

        /// <summary>
        /// Check if the given region is Atlantis.
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public static bool IsAtlantis(int regionId)
        {
            return (regionId == 30 || regionId == 73 || regionId == 130);
        }

        #endregion

        #region Area

        /// <summary>
        /// Adds an area to the region and updates area-zone cache
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public virtual IArea AddArea(IArea area)
        {
            lock (m_lockAreas)
            {
                ushort nextAreaID = 0;

                foreach (ushort areaID in m_Areas.Keys)
                {
                    if (areaID >= nextAreaID)
                    {
                        nextAreaID = (ushort)(areaID + 1);
                    }
                }

                area.ID = nextAreaID;
                m_Areas.Add(area.ID, area);

                int zonePos = 0;
                foreach (Zone zone in Zones)
                {
                    if (area.IsIntersectingZone(zone))
                    	m_ZoneAreas[zonePos][m_ZoneAreasCount[zonePos]++] = area.ID;
                    
                    zonePos++;
                }
                return area;
            }
        }

        /// <summary>
        /// Removes an area from the list of areas and updates area-zone cache
        /// </summary>
        /// <param name="area"></param>
        public virtual void RemoveArea(IArea area)
        {
            lock (m_lockAreas)
            {
                if (m_Areas.ContainsKey(area.ID) == false)
                {
                    return;
                }

                m_Areas.Remove(area.ID);
                int ZoneCount = Zones.Count;

                for (int zonePos = 0; zonePos < ZoneCount; zonePos++)
                {
                    for (int areaPos = 0; areaPos < m_ZoneAreasCount[zonePos]; areaPos++)
                    {
                        if (m_ZoneAreas[zonePos][areaPos] == area.ID)
                        {
                            // move the remaining m_ZoneAreas array one to the left

                            for (int i = areaPos; i < m_ZoneAreasCount[zonePos] - 1; i++)
                            {
                                m_ZoneAreas[zonePos][i] = m_ZoneAreas[zonePos][i + 1];
                            }

                            m_ZoneAreasCount[zonePos]--;
                            break;
                        }
                    }
                }
            }
        }

        [Obsolete("Use .GetAreasOfSpot(Coordinate) instead!")]
        public IList<IArea> GetAreasOfSpot(int x, int y, int z)
            => GetAreasOfSpot(Coordinate.Create(x, y, z));

        public IList<IArea> GetAreasOfSpot(Coordinate coordinate)
        {
            var zone = GetZone(coordinate);
            return GetAreasOfZone(zone, coordinate);
        }

        public virtual IList<IArea> GetAreasOfZone(Zone zone, Coordinate spot)
            => GetAreasOfZone(zone, spot, true);

        public virtual IList<IArea> GetAreasOfZone(Zone zone, Coordinate spot, bool checkZ)
        {
            lock (m_lockAreas)
            {
                int zoneIndex = Zones.IndexOf(zone);
                var areas = new List<IArea>();

                if (zoneIndex >= 0)
                {
                    try
                    {
                        for (int i = 0; i < m_ZoneAreasCount[zoneIndex]; i++)
                        {
                            var area = (IArea)m_Areas[m_ZoneAreas[zoneIndex][i]];
                            if (area.IsContaining(spot, ignoreZ: !checkZ))
                            {
                                areas.Add(area);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error("GetArea exception.Area count " + m_ZoneAreasCount[zoneIndex], e);
                    }
                }

                return areas;
            }
        }
        #endregion

        #region Notify

        public virtual void Notify(GameEvent e, object sender, EventArgs args)
        {
            GameEventManager.Notify(e, sender, args);
        }

        public virtual void Notify(GameEvent e, object sender)
        {
            Notify(e, sender, null);
        }

        public virtual void Notify(GameEvent e)
        {
            Notify(e, null, null);
        }

        public virtual void Notify(GameEvent e, EventArgs args)
        {
            Notify(e, null, args);
        }

        #endregion

        #region Object in Radius (Added by Konik & WitchKing)

        #region New Get in radius
        [Obsolete("Use .GetInRadius(eGameObjectType,Coordinate,ushort,bool,bool) instead!)")]
        protected IEnumerable GetInRadius(Zone.eGameObjectType type, int x, int y, int z, ushort radius, bool withDistance, bool ignoreZ)
            => GetInRadius(type, Coordinate.Create(x, y, z), radius, withDistance, ignoreZ);

        protected IEnumerable GetInRadius(Zone.eGameObjectType type, Coordinate center, ushort radius, bool withDistance, bool ignoreZ)
        {
            // check if we are around borders of a zone
            Zone startingZone = GetZone(center);

            if (startingZone != null)
            {
                ArrayList res = startingZone.GetObjectsInRadius(type, center, radius, new ArrayList(), ignoreZ);

                uint sqRadius = (uint)radius * radius;

                foreach (var currentZone in m_zones)
                {
                    if ((currentZone != startingZone)
                        && (currentZone.TotalNumberOfObjects > 0)
                        && CheckShortestDistance(currentZone, center, sqRadius))
                    {
                        res = currentZone.GetObjectsInRadius(type, center, radius, res, ignoreZ);
                    }
                }

                //Return required enumerator
                IEnumerable tmp = null;
                if (withDistance)
                {
                    switch (type)
                    {
                        case Zone.eGameObjectType.ITEM:
                            tmp = new ItemDistanceEnumerator(center, res);
                            break;
                        case Zone.eGameObjectType.NPC:
                            tmp = new NPCDistanceEnumerator(center, res);
                            break;
                        case Zone.eGameObjectType.PLAYER:
                            tmp = new PlayerDistanceEnumerator(center, res);
                            break;
                        default:
                            tmp = new EmptyEnumerator();
                            break;
                    }
                }
                else
                {
                    tmp = new ObjectEnumerator(res);
                }
                return tmp;
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.Error("GetInRadius starting zone is null for (" + type + ", " + center + ", " + radius + ") in Region ID=" + ID);
                }
                return new EmptyEnumerator();
            }
        }

        private static bool CheckShortestDistance(Zone zone, Coordinate coordinate, uint squareRadius)
        {
            //  coordinates of zone borders
            int xLeft = zone.Offset.X;
            int xRight = zone.Offset.X + zone.Width;
            int yTop = zone.Offset.Y;
            int yBottom = zone.Offset.Y + zone.Height;
            long distance = 0;

            if ((coordinate.Y >= yTop) && (coordinate.Y <= yBottom))
            {
                int xdiff = Math.Min(FastMath.Abs(coordinate.X - xLeft), FastMath.Abs(coordinate.X - xRight));
                distance = (long)xdiff * xdiff;
            }
            else
            {
                if ((coordinate.X >= xLeft) && (coordinate.X <= xRight))
                {
                    int ydiff = Math.Min(FastMath.Abs(coordinate.Y - yTop), FastMath.Abs(coordinate.Y - yBottom));
                    distance = (long)ydiff * ydiff;
                }
                else
                {
                    int xdiff = Math.Min(FastMath.Abs(coordinate.X - xLeft), FastMath.Abs(coordinate.X - xRight));
                    int ydiff = Math.Min(FastMath.Abs(coordinate.Y - yTop), FastMath.Abs(coordinate.Y - yBottom));
                    distance = (long)xdiff * xdiff + (long)ydiff * ydiff;
                }
            }

            return (distance <= squareRadius);
        }

        [Obsolete("Use .GetItemsInRadius(Coordinate,ushort,bool) instead!")]
        public IEnumerable GetItemsInRadius(int x, int y, int z, ushort radius, bool withDistance)
            => GetItemsInRadius(Coordinate.Create(x,y,z), radius, withDistance);

        [Obsolete("Use .GetNPCsInRadius(Coordinate,ushort,bool,bool) instead!")]
        public IEnumerable GetNPCsInRadius(int x, int y, int z, ushort radius, bool withDistance, bool ignoreZ)
            => GetNPCsInRadius(Coordinate.Create(x,y,z), radius, withDistance, ignoreZ);

        [Obsolete("Use .GetPlayersInRadius(Coordinate,ushort,bool,bool) instead!")]
        public IEnumerable GetPlayersInRadius(int x, int y, int z, ushort radius, bool withDistance, bool ignoreZ)
            => GetPlayersInRadius(Coordinate.Create(x,y,z), radius, withDistance, ignoreZ);

        [Obsolete("Use .GetDoorsInRadius(Coordinate,ushort,bool) instead!")]
        public IEnumerable GetDoorsInRadius(int x, int y, int z, ushort radius, bool withDistance)
            => GetDoorsInRadius(Coordinate.Create(x,y,z), radius, withDistance);

        public IEnumerable GetItemsInRadius(Coordinate center, ushort radius, bool withDistance)
            => GetInRadius(Zone.eGameObjectType.ITEM, center, radius, withDistance, false);

        public IEnumerable GetNPCsInRadius(Coordinate center, ushort radius, bool withDistance, bool ignoreZ)
            => GetInRadius(Zone.eGameObjectType.NPC, center, radius, withDistance, ignoreZ);

        public IEnumerable GetPlayersInRadius(Coordinate coordinate, ushort radius, bool withDistance, bool ignoreZ)
            => GetInRadius(Zone.eGameObjectType.PLAYER, coordinate, radius, withDistance, ignoreZ);

        public virtual IEnumerable GetDoorsInRadius(Coordinate center, ushort radius, bool withDistance)
            => GetInRadius(Zone.eGameObjectType.DOOR, center, radius, withDistance, false);

        #endregion

        #region Enumerators

        #region EmptyEnumerator

        /// <summary>
        /// An empty enumerator returned when no objects are found
        /// close to a certain range
        /// </summary>
        public class EmptyEnumerator : IEnumerator, IEnumerable
        {
            /// <summary>
            /// Implementation of the IEnumerable interface
            /// </summary>
            /// <returns>An Enumeration Interface of this class</returns>
            public IEnumerator GetEnumerator()
            {
                return this;
            }

            /// <summary>
            /// Implementation of the IEnumerator interface
            /// </summary>
            /// <returns>Always false to prevent Current</returns>
            public bool MoveNext()
            {
                return false;
            }

            /// <summary>
            /// Implementation of the IEnumerator interface,
            /// always returns null because it shouldn't be
            /// called at all.
            /// </summary>
            public object Current
            {
                get { return null; }
            }

            /// <summary>
            /// Implementation of the IEnumerator interface
            /// </summary>
            public void Reset()
            {
            }
        }

        #endregion

        #region ObjectEnumerator

        /// <summary>
        /// An enumerator over GameObjects. Used to enumerate over
        /// certain objects and do some testing before returning an
        /// object.
        /// </summary>
        public class ObjectEnumerator : IEnumerator, IEnumerable
        {
            /// <summary>
            /// Counter to the current object
            /// </summary>
            protected int m_current = -1;

            protected GameObject[] elements = null;
            //protected ArrayList elements = null;

            protected object m_currentObj = null;

            protected int m_count;

            public IEnumerator GetEnumerator()
            {
                return this;
            }

            public ObjectEnumerator(ArrayList objectSet)
            {
                //objectSet.DumpInfo();
                elements = new GameObject[objectSet.Count];
                objectSet.CopyTo(elements);
                m_count = elements.Length;
            }


            /// <summary>
            /// Get the next GameObjcte from the zone subset created in constructor
            /// and by restrictuing according distance
            /// </summary>
            /// <returns>The Next GameObject of this Enumerator</returns>
            public virtual bool MoveNext()
            {
                /*********NEW GET IN RADIUS SYSTEM ADDED BY KONIK**********/
                m_currentObj = null;
                bool found = false;
                do
                {
                    m_current++;
                    // break if no more object
                    if (m_current < m_count)
                    {
                        // get the object
                        //GameObject obj = (GameObject) elements[m_current];
                        GameObject obj = elements[m_current];
                        if (found = ((obj != null && ((int)obj.ObjectState) == (int)GameObject.eObjectState.Active)))
                        {
                            m_currentObj = obj;
                        }
                    }
                } while (m_current < m_count && !found);
                return found;
            }

            /// <summary>
            /// Returns the current Object in the Enumerator
            /// </summary>
            public virtual object Current
            {
                get { return m_currentObj; }
            }

            /// <summary>
            /// Resets the Enumerator
            /// </summary>
            public void Reset()
            {
                m_currentObj = null;
                m_current = -1;
            }
        }

        #endregion

        #region XXXDistanceEnumerator

        public abstract class DistanceEnumerator : ObjectEnumerator
        {
            protected Coordinate coordinate;

            public DistanceEnumerator(int x, int y, int z, ArrayList elements)
                : this(Coordinate.Create(x, y, z), elements) { }

            public DistanceEnumerator(Coordinate coordinate, ArrayList elements) 
                : base(elements)
            {
                this.coordinate = coordinate;
            }
        }

        public class PlayerDistanceEnumerator : DistanceEnumerator
        {
            public PlayerDistanceEnumerator(int x, int y, int z, ArrayList elements)
                : base(Coordinate.Create(x, y, z), elements) { }

            public PlayerDistanceEnumerator(Coordinate coordinate, ArrayList elements)
                : base(coordinate,elements) { }

            public override object Current
            {
                get
                {
                    GamePlayer obj = (GamePlayer)m_currentObj;
                    return new PlayerDistEntry(obj, (int)obj.Coordinate.DistanceTo(coordinate));
                }
            }
        }

        public class NPCDistanceEnumerator : DistanceEnumerator
        {
            public NPCDistanceEnumerator(int x, int y, int z, ArrayList elements)
                : base(Coordinate.Create(x, y, z), elements) { }

            public NPCDistanceEnumerator(Coordinate coordinate, ArrayList elements)
                : base(coordinate,elements) { }

            public override object Current
            {
                get
                {
                    GameNPC obj = (GameNPC)m_currentObj;
                    return new NPCDistEntry(obj, (int)obj.Coordinate.DistanceTo(coordinate));
                }
            }
        }

        public class ItemDistanceEnumerator : DistanceEnumerator
        {
            public ItemDistanceEnumerator(int x, int y, int z, ArrayList elements)
                : base(Coordinate.Create(x, y, z), elements) { }

            public ItemDistanceEnumerator(Coordinate coordinate, ArrayList elements)
                : base(coordinate,elements) { }

            public override object Current
            {
                get
                {
                    GameStaticItem obj = (GameStaticItem)m_currentObj;
                    return new ItemDistEntry(obj, (int)obj.Coordinate.DistanceTo(coordinate));
                }
            }
        }
        
        #endregion

        #endregion

        #region Automatic relocation

        public void Relocate()
        {
        	foreach (var zone in m_zones)
        	{
        		zone.Relocate(null);
        	}
        	
        	m_lastRelocationTime = DateTime.Now.Ticks / (10 * 1000);
        }

        #endregion

        #endregion

    }
	#region Helpers classes

	/// <summary>
	/// Holds a Object and it's distance towards the center
	/// </summary>
	public class PlayerDistEntry
	{
		public PlayerDistEntry(GamePlayer o, int distance)
		{
			Player = o;
			Distance = distance;
		}

		public GamePlayer Player;
		public int Distance;
	}

	/// <summary>
	/// Holds a Object and it's distance towards the center
	/// </summary>
	public class NPCDistEntry
	{
		public NPCDistEntry(GameNPC o, int distance)
		{
			NPC = o;
			Distance = distance;
		}

		public GameNPC NPC;
		public int Distance;
	}

	/// <summary>
	/// Holds a Object and it's distance towards the center
	/// </summary>
	public class ItemDistEntry
	{
		public ItemDistEntry(GameStaticItem o, int distance)
		{
			Item = o;
			Distance = distance;
		}

		public GameStaticItem Item;
		public int Distance;
	}
#endregion

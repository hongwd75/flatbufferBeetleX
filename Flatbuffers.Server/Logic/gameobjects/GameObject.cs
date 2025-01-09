﻿using System.Collections;
using Game.Logic.Events;
using Game.Logic.Geometry;
using Game.Logic.World;
using Logic.database;

namespace Game.Logic
{
    public class GameObject
    {
        public enum eObjectState : byte
        {
            Active,
            Inactive,
            Deleted
        }
        
        // =============================================================================================
        #region 변수들
        protected eRealm mRealm;

        protected int mObjectID;
        protected bool m_saveInDB;
        protected string m_InternalID; // DB에 있는 유니크 ID
        
        protected byte m_level = 0;
        
        protected volatile eObjectState mObjectState;
        protected Region mCurrentRegion;
        protected string mName;
        protected long m_spawnTick = 0;
        #endregion

        #region ======== GET / SET =====================================================================================
        /// <summary>
        /// Gets or Sets the current level of the Object
        /// </summary>
        public virtual byte Level
        {
	        get { return m_level; }
	        set { m_level = value; }
        }

        public virtual int EffectiveLevel
        {
	        get { return Level; }
	        set { }
        }

        public virtual byte GetDisplayLevel(GamePlayer player)
        {
	        return Level;
        }
        
        public virtual eRealm Realm
        {
            get => mRealm;
            set
            {
                mRealm = value;
            }
        }
        
        public virtual eGender Gender
        {
	        get { return eGender.Neutral; }
	        set { }
        }
        
        public virtual int ObjectID
        {
            get => mObjectID;
            set
            {
                mObjectID = value;
            } 
        }
        
        public long SpawnTick
        {
	        get { return m_spawnTick; }
        }
        
        public virtual string InternalID
        {
            get { return m_InternalID; }
            set { m_InternalID = value; }
        }
        
        public bool SaveInDB
        {
            get { return m_saveInDB; }
            set { m_saveInDB = value; }
        } 
        
        public virtual void SaveIntoDatabase()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public virtual void LoadFromDatabase(DataObject obj)
        {
            InternalID = obj.ObjectId;
        }

        /// <summary>
        /// Deletes a character from the DB
        /// </summary>
        public virtual void DeleteFromDatabase()
        {
        }
        
        public virtual eObjectState ObjectState
        {
            get => mObjectState;
            set
            {
                mObjectState = value;
            }
        }
        
        public virtual Region CurrentRegion
        {
	        get => Position.Region;
	        set
	        {
		        if(value == null) Position = Position.With(regionID: 0);
		        else Position = Position.With(regionID: value.ID);
	        }
        }
        
        public Zone CurrentZone
        {
	        get
	        {
		        if (CurrentRegion != null)
		        {
			        return CurrentRegion.GetZone(Coordinate);
		        }
		        return null;
	        }
        }
        
        public virtual Position Position { get; set; }
        public virtual Angle Orientation
        {
	        get => Position.Orientation;
	        set => Position = Position.With(value);
        }
        
        public Angle GetAngleTo(Coordinate coordinate)
	        => Coordinate.GetOrientationTo(coordinate) - Orientation;
        public int GetDistanceTo(GameObject obj, double zfactor = 1)
	        => GetDistanceTo(obj.Position, zfactor);        
        
        public Coordinate Coordinate => Position.Coordinate;

        public virtual ushort CurrentRegionID
        {
	        get => Position.RegionID;
	        set => Position = Position.With(regionID: value);
        }
        

        
        public string Name
        {
            get => mName;
            set
            {
                mName = value;
            }
        }
        #endregion

        #region ========= Health =======================================================================================
        protected int m_health;
        public virtual int Health
        {
	        get { return m_health; }
	        set
	        {

		        int maxhealth = MaxHealth;
		        if (value >= maxhealth)
		        {
			        m_health = maxhealth;
		        }
		        else if (value > 0)
		        {
			        m_health = value;
		        }
		        else
		        {
			        m_health = 0;
		        }
	        }
        }

        protected int m_maxHealth;
        public virtual int MaxHealth
        {
	        get { return m_maxHealth; }
	        set
	        {
		        m_maxHealth = value;
	        }
        }

        public virtual byte HealthPercent
        {
	        get
	        {
		        return (byte)(MaxHealth <= 0 ? 0 : Health * 100 / MaxHealth);
	        }
        }
        #endregion
        
        #region ========= Notify =======================================================================================
        public virtual void Notify(GameEvent e, object sender, EventArgs args) => GameEventManager.Notify(e, sender, args);
        public virtual void Notify(GameEvent e, object sender) => Notify(e, sender, null);
        public virtual void Notify(GameEvent e) => Notify(e, null, null);
        public virtual void Notify(GameEvent e, EventArgs args) => Notify(e, null, args);
        #endregion

        #region ======== 범위 검색 ======================================================================================
        public virtual int GetDistanceTo(Position position, double zfactor)
        {
	        if (Position.RegionID != position.RegionID) return int.MaxValue;

	        var offset = position.Coordinate - Coordinate;
	        var dz = offset.Z * zfactor;

	        return (int)(offset.Length2D + Math.Sqrt(dz*dz));
        }
        
        public bool IsWithinRadius(GameObject obj, int radius, bool ignoreZ = false)
        {
	        if (obj == null) return false;

	        if (this.CurrentRegionID != obj.CurrentRegionID) return false;

	        double distance;
	        if (ignoreZ) distance = Coordinate.DistanceTo(obj.Coordinate, ignoreZ: true);
	        else distance = Coordinate.DistanceTo(obj.Coordinate);

	        return distance < radius;
        }
        
        public virtual bool IsObjectInFront(GameObject target, double viewangle, bool rangeCheck = true)
        {
	        if (target == null)
		        return false;
	        var angle = GetAngleTo(target.Coordinate);
	        var isInFront = angle.InDegrees >= 360 - viewangle / 2 || angle.InDegrees < viewangle / 2;
	        if (isInFront) return true;

	        if (rangeCheck)
		        return this.IsWithinRadius( target, 32 );
	        else
		        return false;
        }
        
        public virtual IList<IArea> CurrentAreas
        {
	        get
	        {
		        if (CurrentZone != null)
			        return CurrentZone.GetAreasOfSpot(Coordinate);
		        return new List<IArea>();
	        }
	        set { }
        }
        
        public virtual bool IsUnderwater
        {
	        get
	        {
		        if (CurrentRegion == null || CurrentZone == null)
			        return false;
		        return Position.Z < CurrentZone.Waterlevel;
	        }
        }
        
        public virtual bool IsVisibleTo(GameObject checkObject)
        {
	        if (checkObject == null || CurrentRegion != checkObject.CurrentRegion)
	        {
		        return false;
	        }

	        return true;
        }
        
		public IEnumerable GetNPCsInRadius(bool useCache, ushort radiusToCheck)
		{
			return GetNPCsInRadius(useCache, radiusToCheck, false, false);
		}

		public IEnumerable GetNPCsInRadius(bool useCache, ushort radiusToCheck, bool ignoreZ)
		{
			return GetNPCsInRadius(useCache, radiusToCheck, false, ignoreZ);
		}
		
		public IEnumerable GetNPCsInRadius(bool useCache, ushort radiusToCheck, bool withDistance, bool ignoreZ)
		{
			if (CurrentRegion != null)
			{
				//Eden - avoid server freeze
				if (CurrentZone == null)
				{
					if (this is GamePlayer && !(this as GamePlayer).TempProperties.getProperty("isbeingbanned", false))
					{
						GamePlayer player = this as GamePlayer;
						player.TempProperties.setProperty("isbeingbanned", true);
						player.MoveToBind();
					}
				}
				else
				{
					IEnumerable result = CurrentRegion.GetNPCsInRadius(Coordinate, radiusToCheck, withDistance, ignoreZ);
					return result;
				}
			}

			return new Region.EmptyEnumerator();
		}
		
		public IEnumerable GetItemsInRadius(ushort radiusToCheck)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			return GetItemsInRadius(radiusToCheck, false);
			/***************************************************************/
		}
		
		public IEnumerable GetItemsInRadius(ushort radiusToCheck, bool withDistance)
		{
			/******* MODIFIED BY KONIK & WITCHKING FOR NEW ZONE SYSTEM *********/
			if (CurrentRegion != null)
			{
				//Eden - avoid server freeze
				if (CurrentZone == null)
				{
					if (this is GamePlayer && !(this as GamePlayer).TempProperties.getProperty("isbeingbanned", false))
					{
						GamePlayer player = this as GamePlayer;
						player.TempProperties.setProperty("isbeingbanned", true);
						player.MoveToBind();
					}
				}
				else
				{
					return CurrentRegion.GetItemsInRadius(Coordinate, radiusToCheck, withDistance);
				}
			}
			return new Region.EmptyEnumerator();
			/***************************************************************/
		}
		public virtual IEnumerable GetPlayersInRadius(ushort radiusToCheck)
		{
			return GetPlayersInRadius(false, radiusToCheck, false, false);
		}


		public IEnumerable GetPlayersInRadius(ushort radiusToCheck, bool ignoreZ)
		{
			return GetPlayersInRadius(false, radiusToCheck, false, ignoreZ);
		}

		public IEnumerable GetPlayersInRadius(bool useCache, ushort radiusToCheck)
		{
			return GetPlayersInRadius(useCache, radiusToCheck, false, false);
		}
		
		public IEnumerable GetPlayersInRadius(bool useCache, ushort radiusToCheck, bool ignoreZ)
		{
			return GetPlayersInRadius(useCache, radiusToCheck, false, ignoreZ);
		}

		public IEnumerable GetPlayersInRadius(ushort radiusToCheck, bool withDistance, bool ignoreZ)
		{
			return GetPlayersInRadius(true, radiusToCheck, withDistance, ignoreZ);
		}

		public IEnumerable GetPlayersInRadius(bool useCache, ushort radiusToCheck, bool withDistance, bool ignoreZ)
		{
			if (CurrentRegion != null)
			{
				//Eden - avoid server freeze
				if (CurrentZone == null)
				{
					if (this is GamePlayer && (this as GamePlayer).Network.Account.PrivLevel < 3 && !(this as GamePlayer).TempProperties.getProperty("isbeingbanned", false))
					{
						GamePlayer player = this as GamePlayer;
						player.TempProperties.setProperty("isbeingbanned", true);
						player.MoveToBind();
					}
				}
				else
				{
					return CurrentRegion.GetPlayersInRadius(Coordinate, radiusToCheck, withDistance, ignoreZ);
				}
			}
			return new Region.EmptyEnumerator();
		}

		public IEnumerable GetNPCsInRadius(ushort radiusToCheck)
		{
			return GetNPCsInRadius(true, radiusToCheck, false, false);
		}

		public IEnumerable GetNPCsInRadius(ushort radiusToCheck, bool ignoreZ)
		{
			return GetNPCsInRadius(true, radiusToCheck, false, ignoreZ);
		}		
        #endregion
        
        #region ConLevel/DurLevel

        /// <summary>
        /// Calculate con-level against other object
        /// &lt;=-3 = grey
        /// -2 = green
        /// -1 = blue
        /// 0 = yellow (same level)
        /// 1 = orange
        /// 2 = red
        /// &gt;=3 = violet
        /// </summary>
        /// <returns>conlevel</returns>
        public double GetConLevel(GameObject compare)
        {
	        return GetConLevel(EffectiveLevel, compare.EffectiveLevel);
	        //			return (compare.Level - Level) / ((double)(Level / 10 + 1));
        }

        /// <summary>
        /// Calculate con-level against other compareLevel
        /// -3- = grey
        /// -2 = green
        /// -1 = blue  (compareLevel is 1 con lower)
        /// 0 = yellow (same level)
        /// 1 = orange (compareLevel is 1 con higher)
        /// 2 = red
        /// 3+ = violet
        /// </summary>
        /// <returns>conlevel</returns>
        public static double GetConLevel(int level, int compareLevel)
        {
	        int constep = Math.Max(1, (level + 9) / 10);
	        double stepping = 1.0 / constep;
	        int leveldiff = level - compareLevel;
	        return 0 - leveldiff * stepping;
        }

        /// <summary>
        /// Calculate a level based on source level and a con level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        public static int GetLevelFromCon(int level, double con)
        {
	        int constep = Math.Max(1, (level + 10) / 10);
	        return Math.Max((int)0, (int)(level + constep * con));
        }

        #endregion
        
        public virtual bool AddToWorld()
        {
	        Zone currentZone = CurrentZone;
	        if (currentZone == null || mObjectState == eObjectState.Active)
		        return false;

	        if (!CurrentRegion.AddObject(this))
	        {
		        return false;
	        }
	        
	        Notify(GameObjectEvent.AddToWorld, this);
	        
	        ObjectState = eObjectState.Active;
	        CurrentZone.ObjectEnterZone(this);
	        m_spawnTick = CurrentRegion.Time;
	        return true;
        }
        
        public virtual bool RemoveFromWorld()
        {
	        if (CurrentRegion == null || ObjectState != eObjectState.Active)
		        return false;
	        Notify(GameObjectEvent.RemoveFromWorld, this);
	        ObjectState = eObjectState.Inactive;
	        CurrentRegion.RemoveObject(this);
	        return true;
        }
        
        public virtual bool MoveTo(Position position)
        {
	        if (mObjectState != eObjectState.Active)
		        return false;

	        Region rgn = WorldManager.GetRegion(position.RegionID);
	        if (rgn == null)
		        return false;
	        if (rgn.GetZone(position.Coordinate) == null)
		        return false;

	        if (!RemoveFromWorld())
		        return false;
	        Position = position;
	        return AddToWorld();
        }

        public virtual void Delete()
        {
	        Notify(GameObjectEvent.Delete, this);
	        RemoveFromWorld();
	        ObjectState = eObjectState.Deleted;
	        GameEventManager.RemoveAllHandlersForObject(this);
        }        
    }
}
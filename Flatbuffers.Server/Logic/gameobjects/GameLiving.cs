using System.Net.Http.Headers;
using Game.Logic.AI.Brain;
using Game.Logic.Events;
using Game.Logic.Geometry;
using Game.Logic.PropertyCalc;
using Game.Logic.Skills;
using Game.Logic.Utils;
using Game.Logic.World;
using Game.Logic.World.Timer;

namespace Game.Logic;

public class GameLiving : GameObject
{
	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
    private readonly PropertyCollection m_tempProps = new PropertyCollection();
    internal static readonly IPropertyCalculator[] m_propertyCalc = new IPropertyCalculator[(int)eProperty.MaxProperty+1];

	#region ==== ENUM ======================================================================================================
	public enum eActiveWeaponSlot : byte
	{
		/// <summary>
		/// Weapon slot righthand
		/// </summary>
		Standard = 0x00,
		/// <summary>
		/// Weaponslot twohanded
		/// </summary>
		TwoHanded = 0x01,
		/// <summary>
		/// Weaponslot distance
		/// </summary>
		Distance = 0x02
	}
	#endregion
	
	
    #region ==== 스탯 관련 함수 =========================================================================================
    protected readonly short[] m_charStat = new short[8];
    protected IPropertyIndexer[] m_PropertyIndexers = new IPropertyIndexer[(int)eBuffBonusType.MaxBonusType];
    protected IMultiplicativeProperties m_buffMultBonus = new MultiplicativeProperties();
    
    public virtual IPropertyIndexer GetBuffBonus(eBuffBonusType type)
    {
        return m_PropertyIndexers[(int)type];
    }
    public IMultiplicativeProperties BuffBonusMultCategory
    {
        get { return m_buffMultBonus; }
    }
    
    public virtual int GetBaseStat(eStat stat)
    {
        return m_charStat[stat - eStat._First];
    }
    public virtual void ChangeBaseStat(eStat stat, short amount)
    {
        m_charStat[stat - eStat._First] += amount;
    }
    
    protected eActiveWeaponSlot m_activeWeaponSlot;
    protected AttackAction m_attackAction;
    protected readonly List<GameObject> m_attackers;
    public virtual eActiveWeaponSlot ActiveWeaponSlot
    {
	    get { return m_activeWeaponSlot; }
    }    
    protected virtual AttackAction CreateAttackAction()
    {
	    return m_attackAction ?? new AttackAction(this);
    }
    
    protected readonly GameEffectList m_effects;
    public GameEffectList EffectList
    {
	    get { return m_effects; }
    }
    protected virtual GameEffectList CreateEffectsList()
    {
	    return new GameEffectList(this);
    }    
    #endregion
    
    public PropertyCollection TempProperties
    {
        get { return m_tempProps; }
    }
    
    protected short m_race;
    public virtual short Race
    {
        get { return m_race; }
        set { m_race = value; }
    }    

    #region ControlledNpc

    private byte m_petCount = 0;

    /// <summary>
    /// Gets the pet count for this living
    /// </summary>
    public byte PetCount
    {
	    get { return m_petCount; }
	    set { m_petCount = value; }
    }

    /// <summary>
    /// Holds the controlled object
    /// </summary>
    protected IControlledBrain[] m_controlledBrain = null;

    /// <summary>
    /// Initializes the ControlledNpcs for the GameLiving class
    /// </summary>
    /// <param name="num">Number of places to allocate.  If negative, sets to null.</param>
    public virtual void InitControlledBrainArray(int num)
    {
	    if (num > 0)
	    {
		    m_controlledBrain = new IControlledBrain[num];
	    }
	    else
	    {
		    m_controlledBrain = null;
	    }
    }

    /// <summary>
    /// Get or set the ControlledBrain.  Set always uses m_controlledBrain[0]
    /// </summary>
    public virtual IControlledBrain ControlledBrain
    {
	    get
	    {
		    if (m_controlledBrain == null)
			    return null;

		    return m_controlledBrain[0];
	    }
	    set
	    {
		    m_controlledBrain[0] = value;
	    }
    }

    /// <summary>
    /// Get the controlled pet's body, or null if not present.  Always uses m_controlledBrain[0]
    /// </summary>
    public virtual GameLiving ControlledBody
    {
	    get
	    {
		    if (m_controlledBrain != null && m_controlledBrain[0] != null && m_controlledBrain[0].Body is GameLiving body)
			    return body;
		    return null;
	    }
    }

    public virtual bool IsControlledNPC(GameNPC npc)
    {
	    if (npc == null)
	    {
		    return false;
	    }
	    IControlledBrain brain = npc.Brain as IControlledBrain;
	    if (brain == null)
	    {
		    return false;
	    }
	    return brain.GetLivingOwner() == this;
    }

    /// <summary>
    /// Sets the controlled object for this player
    /// </summary>
    /// <param name="controlledNpc"></param>
    public virtual void SetControlledBrain(IControlledBrain controlledBrain)
    {
    }
    #endregion

    #region GetModifieds
    public virtual int GetModified(eProperty property)
    {
	    if (m_propertyCalc != null && m_propertyCalc[(int)property] != null)
	    {
		    return m_propertyCalc[(int)property].CalcValue(this, property);
	    }
	    else
	    {
		    log.ErrorFormat("{0} did not find property calculator for property ID {1}.", Name, (int)property);
	    }
	    return 0;
    }
    public virtual int GetModifiedBase(eProperty property)
    {
	    if (m_propertyCalc != null && m_propertyCalc[(int)property] != null)
	    {
		    return m_propertyCalc[(int)property].CalcValueBase(this, property);
	    }
	    else
	    {
		    log.ErrorFormat("{0} did not find base property calculator for property ID {1}.", Name, (int)property);
	    }
	    return 0;
    }
    public virtual int GetModifiedFromBuffs(eProperty property)
    {
	    if (m_propertyCalc != null && m_propertyCalc[(int)property] != null)
	    {
		    return m_propertyCalc[(int)property].CalcValueFromBuffs(this, property);
	    }
	    else
	    {
		    log.ErrorFormat("{0} did not find buff property calculator for property ID {1}.", Name, (int)property);
	    }
	    return 0;
    }
    public virtual int GetModifiedFromItems(eProperty property)
    {
	    if (m_propertyCalc != null && m_propertyCalc[(int)property] != null)
	    {
		    return m_propertyCalc[(int)property].CalcValueFromItems(this, property);
	    }
	    else
	    {
		    log.ErrorFormat("{0} did not find item property calculator for property ID {1}.", Name, (int)property);
	    }
	    return 0;
    }
    #endregion

	#region Regeneration
    protected RegionTimer m_healthRegenerationTimer;
	protected RegionTimer m_powerRegenerationTimer;
	protected RegionTimer m_enduRegenerationTimer;
	protected const ushort m_healthRegenerationPeriod = 3000;
	protected virtual ushort HealthRegenerationPeriod
	{
		get { return m_healthRegenerationPeriod; }
	}
	protected const ushort m_powerRegenerationPeriod = 3000;
	protected virtual ushort PowerRegenerationPeriod
	{
		get { return m_powerRegenerationPeriod; }
	}
	protected const ushort m_enduranceRegenerationPeriod = 1000;
	protected virtual ushort EnduranceRegenerationPeriod
	{
		get { return m_enduranceRegenerationPeriod; }
	}
	protected readonly object m_regenTimerLock = new object();
	
	public virtual void StartHealthRegeneration()
	{
		if (ObjectState != eObjectState.Active)
			return;
		lock (m_regenTimerLock)
		{
			if (m_healthRegenerationTimer == null)
			{
				m_healthRegenerationTimer = new RegionTimer(this);
				m_healthRegenerationTimer.Callback = new RegionTimerCallback(HealthRegenerationTimerCallback);
			}
			else if (m_healthRegenerationTimer.IsAlive)
			{
				return;
			}

			m_healthRegenerationTimer.Start(HealthRegenerationPeriod);
		}
	}
	public virtual void StartPowerRegeneration()
	{
		if (ObjectState != eObjectState.Active)
			return;
		lock (m_regenTimerLock)
		{
			if (m_powerRegenerationTimer == null)
			{
				m_powerRegenerationTimer = new RegionTimer(this);
				m_powerRegenerationTimer.Callback = new RegionTimerCallback(PowerRegenerationTimerCallback);
			}
			else if (m_powerRegenerationTimer.IsAlive)
			{
				return;
			}

			m_powerRegenerationTimer.Start(PowerRegenerationPeriod);
		}
	}
	public virtual void StartEnduranceRegeneration()
	{
		if (ObjectState != eObjectState.Active)
			return;
		lock (m_regenTimerLock)
		{
			if (m_enduRegenerationTimer == null)
			{
				m_enduRegenerationTimer = new RegionTimer(this);
				m_enduRegenerationTimer.Callback = new RegionTimerCallback(EnduranceRegenerationTimerCallback);
			}
			else if (m_enduRegenerationTimer.IsAlive)
			{
				return;
			}
			m_enduRegenerationTimer.Start(EnduranceRegenerationPeriod);
		}
	}
	public virtual void StopHealthRegeneration()
	{
		lock (m_regenTimerLock)
		{
			if (m_healthRegenerationTimer == null)
				return;
			m_healthRegenerationTimer.Stop();
			m_healthRegenerationTimer = null;
		}
	}
	public virtual void StopPowerRegeneration()
	{
		lock (m_regenTimerLock)
		{
			if (m_powerRegenerationTimer == null)
				return;
			m_powerRegenerationTimer.Stop();
			m_powerRegenerationTimer = null;
		}
	}
	public virtual void StopEnduranceRegeneration()
	{
		lock (m_regenTimerLock)
		{
			if (m_enduRegenerationTimer == null)
				return;
			m_enduRegenerationTimer.Stop();
			m_enduRegenerationTimer = null;
		}
	}
	protected virtual int HealthRegenerationTimerCallback(RegionTimer callingTimer)
	{
		if (Health < MaxHealth)
		{
			ChangeHealth(this, eChargeChangeType.Regenerate, GetModified(eProperty.HealthRegenerationRate));
		}

		//If we are fully healed, we stop the timer
		if (Health >= MaxHealth)
		{
			//We clean all damagedealers if we are fully healed,
			//no special XP calculations need to be done
			lock (m_xpGainers.SyncRoot)
			{
				m_xpGainers.Clear();
			}

			return 0;
		}

		if (InCombat)
		{
			// in combat each tic is aprox 15 seconds - tolakram
			return HealthRegenerationPeriod * 5;
		}

		//Heal at standard rate
		return HealthRegenerationPeriod;
	}
	/// <summary>
	/// Callback for the power regenerationTimer
	/// </summary>
	/// <param name="selfRegenerationTimer">timer calling this function</param>
	protected virtual int PowerRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
	{
		if (this is GamePlayer &&
		    (((GamePlayer)this).CharacterClass.ID == (int)eCharacterClass.Vampiir ||
		     (((GamePlayer)this).CharacterClass.ID > 59 && ((GamePlayer)this).CharacterClass.ID < 63))) // Maulers
		{
			double MinMana = MaxMana * 0.15;
			double OnePercMana = Math.Ceiling(MaxMana * 0.01);

			if (!InCombat)
			{
				if (ManaPercent < 15)
				{
					ChangeMana(this, eChargeChangeType.Regenerate, (int)OnePercMana);
					return 4000;
				}
				else if (ManaPercent > 15)
				{
					ChangeMana(this, eChargeChangeType.Regenerate, (int)(-OnePercMana));
					return 1000;
				}

				return 0;
			}
		}
		else
		{
			if (Mana < MaxMana)
			{
				ChangeMana(this, eChargeChangeType.Regenerate, GetModified(eProperty.PowerRegenerationRate));
			}

			//If we are full, we stop the timer
			if (Mana >= MaxMana)
			{
				return 0;
			}
		}

		//If we were hit before we regenerated, we regenerate slower the next time
		if (InCombat)
		{
			return (int)(PowerRegenerationPeriod * 3.4);
		}

		//regen at standard rate
		return PowerRegenerationPeriod;
	}
	/// <summary>
	/// Callback for the endurance regenerationTimer
	/// </summary>
	/// <param name="selfRegenerationTimer">timer calling this function</param>
	protected virtual int EnduranceRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
	{
		if (Endurance < MaxEndurance)
		{
			int regen = GetModified(eProperty.EnduranceRegenerationRate);
			if (regen > 0)
			{
				ChangeEndurance(this, eChargeChangeType.Regenerate, regen);
			}
		}
		if (Endurance >= MaxEndurance) return 0;

		return 500 + RandomUtil.Int(EnduranceRegenerationPeriod);
	}
	
	public virtual int ChangeHealth(GameObject changeSource, eChargeChangeType healthChangeType, int changeAmount)
	{
		//TODO fire event that might increase or reduce the amount
		int oldHealth = Health;
		Health += changeAmount;
		int healthChanged = Health - oldHealth;

		//Notify our enemies that we were healed by other means than
		//natural regeneration, this allows for aggro on healers!
		if (healthChanged > 0 && healthChangeType != eChargeChangeType.Regenerate)
		{
			IList<GameObject> attackers;
			lock (Attackers) { attackers = new List<GameObject>(m_attackers); }
			EnemyHealedEventArgs args = new EnemyHealedEventArgs(this, changeSource, healthChangeType, healthChanged);
			foreach (GameObject attacker in attackers)
			{
				if (attacker is GameLiving)
				{
					(attacker as GameLiving).Notify(GameLivingEvent.EnemyHealed, attacker, args);
				}
			}
		}
		return healthChanged;
	}

	public virtual int ChangeMana(GameObject changeSource, eChargeChangeType manaChangeType, int changeAmount)
	{
		int oldMana = Mana;
		Mana += changeAmount;
		return Mana - oldMana;
	}
	public virtual int ChangeEndurance(GameObject changeSource, eChargeChangeType enduranceChangeType, int changeAmount)
	{
		int oldEndurance = Endurance;
		Endurance += changeAmount;
		return Endurance - oldEndurance;
	}
	public virtual void EnemyHealed(GameLiving enemy, GameObject healSource, eChargeChangeType changeType, int healAmount)
	{
		Notify(GameLivingEvent.EnemyHealed, this, new EnemyHealedEventArgs(enemy, healSource, changeType, healAmount));
	}	
	#endregion
    
#region Mana/Health/Endurance/Concentration/Delete
	protected int m_mana;
	protected int m_endurance;
	protected int m_maxEndurance;

	public virtual bool IsAlive
	{
		get { return Health > 0; }
	}

	public virtual bool IsLowHealth
	{
		get
		{
			return (Health < 0.1 * MaxHealth);
		}
	}
	
	public override int Health
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

			if (IsAlive && m_health < maxhealth)
			{
				StartHealthRegeneration();
			}
		}
	}

	public override int MaxHealth
	{
		get {	return GetModified(eProperty.MaxHealth); }
	}

	public virtual int Mana
	{
		get
		{
			return m_mana;
		}
		set
		{
			int maxmana = MaxMana;
			m_mana = Math.Min(value, maxmana);
			m_mana = Math.Max(m_mana, 0);
			if (IsAlive && (m_mana < maxmana || (this is GamePlayer && ((GamePlayer)this).CharacterClass.ID == (int)eCharacterClass.Vampiir)
			                || (this is GamePlayer && ((GamePlayer)this).CharacterClass.ID > 59 && ((GamePlayer)this).CharacterClass.ID < 63)))
			{
				StartPowerRegeneration();
			}
		}
	}

	public virtual int MaxMana
	{
		get
		{
			return GetModified(eProperty.MaxMana);
		}
	}

	public virtual byte ManaPercent
	{
		get
		{
			return (byte)(MaxMana <= 0 ? 0 : ((Mana * 100) / MaxMana));
		}
	}

	/// <summary>
	/// Gets/sets the object endurance
	/// </summary>
	public virtual int Endurance
	{
		get { return m_endurance; }
		set
		{
			m_endurance = Math.Min(value, m_maxEndurance);
			m_endurance = Math.Max(m_endurance, 0);
			if (IsAlive && m_endurance < m_maxEndurance)
			{
				StartEnduranceRegeneration();
			}
		}
	}

	/// <summary>
	/// Gets or sets the maximum endurance of this living
	/// </summary>
	public virtual int MaxEndurance
	{
		get { return m_maxEndurance; }
		set
		{
			m_maxEndurance = value;
			Endurance = Endurance; //cut extra end points if there are any or start regeneration
		}
	}

	/// <summary>
	/// Gets the endurance in percent of maximum
	/// </summary>
	public virtual byte EndurancePercent
	{
		get
		{
			return (byte)(MaxEndurance <= 0 ? 0 : ((Endurance * 100) / MaxEndurance));
		}
	}
	#endregion
		
#region Speed/Heading/Target/GroundTarget/GuildName/SitState/Level
		/// <summary>
		/// The targetobject of this living
		/// This is a weak reference to a GameObject, which
		/// means that the gameobject can be cleaned up even
		/// when this living has a reference on it ...
		/// </summary>
		protected readonly WeakReference m_targetObjectWeakReference;
		/// <summary>
		/// The current speed of this living
		/// </summary>
		protected short m_currentSpeed;
		/// <summary>
		/// The base maximum speed of this living
		/// </summary>
		protected short m_maxSpeedBase;

		private bool m_fixedSpeed = false;

		/// <summary>
		/// Does this NPC have a fixed speed, unchanged by any modifiers?
		/// </summary>
		public virtual bool FixedSpeed
		{
			get { return m_fixedSpeed; }
			set { m_fixedSpeed = value; }
		}

        public virtual short CurrentSpeed
        {
            get => Motion.Speed;
            set => Motion = Geometry.Motion.Create(Position, Motion.Destination, value);
        }

		/// <summary>
		/// Gets the maxspeed of this living
		/// </summary>
		public virtual short MaxSpeed
		{
			get
			{
				if (FixedSpeed)
					return MaxSpeedBase;

				return (short)GetModified(eProperty.MaxSpeed);
			}
		}

		/// <summary>
		/// Gets or sets the base max speed of this living
		/// </summary>
		public virtual short MaxSpeedBase
		{
			get { return m_maxSpeedBase; }
			set { m_maxSpeedBase = value; }
		}

		/// <summary>
		/// Gets or sets the target of this living
		/// </summary>
		public virtual GameObject TargetObject
		{
			get
			{
				return (m_targetObjectWeakReference.Target as GameObject);
			}
			set
			{
				m_targetObjectWeakReference.Target = value;
			}
		}

        public virtual void TurnTo(Coordinate coordinate, bool sendUpdate = true)
            => Orientation = Coordinate.GetOrientationTo(coordinate);

		public virtual bool IsSitting
		{
			get { return false; }
			set { }
		}

        [Obsolete("Use GroundTargetPosition instead!")]
        public virtual Point3D GroundTarget
            => GroundTargetPosition.Coordinate.ToPoint3D();

        [Obsolete("Use GroundTargetPosition_set instead!")]
        public virtual void SetGroundTarget(int groundX, int groundY, int groundZ)
            => GroundTargetPosition = Position.Create(Position.RegionID, groundX, groundY, groundZ);

        public virtual Position GroundTargetPosition { get; set; } = Position.Nowhere;

		/// <summary>
		/// Gets or Sets the current level of the Object
		/// </summary>
		public override byte Level
		{
			get { return base.Level; }
			set
			{
				base.Level = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						if (player == null)
							continue;
						player.Out.SendLivingDataUpdate(this, false);
					}
				}
			}
		}

		/// <summary>
		/// What is the base, unmodified level of this living
		/// </summary>
		public virtual byte BaseLevel
		{
			get { return Level; }
		}

		/// <summary>
		/// Calculates the level of a skill on this living.  Generally this is simply the level of the skill.
		/// </summary>
		public virtual int CalculateSkillLevel(Skill skill)
		{
			return skill.Level;
		}
		#endregion
		
		#region Movement
		// public virtual void UpdateHealthManaEndu()
		// {
		// 	if (IsAlive)
		// 	{
		// 		if (Health < MaxHealth) StartHealthRegeneration();
		// 		else if (Health > MaxHealth) Health = MaxHealth;
		//
		// 		if (Mana < MaxMana) StartPowerRegeneration();
		// 		else if (Mana > MaxMana) Mana = MaxMana;
		//
		// 		if (Endurance < MaxEndurance) StartEnduranceRegeneration();
		// 		else if (Endurance > MaxEndurance) Endurance = MaxEndurance;
		// 	}
		// }

		public int MovementStartTick
			=> Motion.StartTimeInMilliSeconds;

		public virtual bool IsMoving => CurrentSpeed != 0;

		public override Position Position
		{
			get => Motion.CurrentPosition;
			set => Motion = Motion.Create(value, Motion.Destination, Motion.Speed);
		}

		protected virtual Motion Motion { get; set; } = new Motion();

		public override Angle Orientation 
		{
			get => Position.Orientation;
			set => Position = Motion.Start.With(orientation: value);
		}

		public override bool MoveTo(Position position)
		{
			if (position.RegionID != CurrentRegionID) CancelAllConcentrationEffects();
			return base.MoveTo(position);
		}
		#endregion		
    /// <summary>
    /// 생성 초기화
    /// </summary>
    public GameLiving() : base()
    {
        for (eBuffBonusType i = 0; i < eBuffBonusType.MaxBonusType; i++)
        {
            m_PropertyIndexers[(int)i] = i switch
            {
                _=> new PropertyIndexer()
            };
        }
        
        m_targetObjectWeakReference = new WeakRef(null);
        
        m_health = 1;
        m_mana = 1;
        m_endurance = 1;
        m_maxEndurance = 1;        
    }
}
using System.Collections;
using System.Net.Http.Headers;
using Game.Logic.AI.Brain;
using Game.Logic.Effects;
using Game.Logic.Events;
using Game.Logic.Geometry;
using Game.Logic.Inventory;
using Game.Logic.Language;
using Game.Logic.PropertyCalc;
using Game.Logic.Skills;
using Game.Logic.Spells;
using Game.Logic.Styles;
using Game.Logic.Utils;
using Game.Logic.World;
using Game.Logic.World.Timer;
using Logic.database.table;
using NetworkMessage;
/*
	RangedAttackState, RangedAttackType 은 제외함.   
 */
namespace Game.Logic;

public partial class GameLiving : GameObject
{
	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
    private readonly PropertyCollection m_tempProps = new PropertyCollection();
    internal static readonly IPropertyCalculator[] m_propertyCalc = new IPropertyCalculator[(int)eProperty.MaxProperty+1];


	#region ==== ENUM ======================================================================================================

	public enum eActiveWeaponSlot : byte
	{
		Standard = 0x00,

		/// Weapon slot righthand
		TwoHanded = 0x01,

		/// Weaponslot twohanded
		Distance = 0x02 /// Weaponslot distance
	};

	public enum eAttackResult : int
	{
		Any = 0,
		HitUnstyled = 1,
		HitStyle = 2,
		NotAllowed_ServerRules = 3,
		NoTarget = 5,
		TargetDead = 6,
		OutOfRange = 7,
		Missed = 8,
		Evaded = 9,
		Blocked = 10,
		Parried = 11,
		NoValidTarget = 12,
		TargetNotVisible = 14,
		Fumbled = 15,
		Bodyguarded = 16,
		Phaseshift = 17,
		Grappled = 18
	};
	
	protected static readonly eProperty[] m_damageTypeToResistBonusConversion = new eProperty[] {
		eProperty.Resist_Natural, //0,
		eProperty.Resist_Crush,
		eProperty.Resist_Slash,
		eProperty.Resist_Thrust,
		0, 0, 0, 0, 0, 0,
		eProperty.Resist_Body,
		eProperty.Resist_Cold,
		eProperty.Resist_Energy,
		eProperty.Resist_Heat,
		eProperty.Resist_Matter,
		eProperty.Resist_Spirit
	};	
	#endregion
	
	
    #region ==== 스탯 관련 함수 =========================================================================================
    protected readonly short[] m_charStat = new short[8];
    protected IPropertyIndexer[] m_PropertyIndexers = new IPropertyIndexer[(int)eBuffBonusType.MaxBonusType];
    protected IMultiplicativeProperties m_buffMultBonus = new MultiplicativeProperties();
    
    public virtual IPropertyIndexer GetBuffBonus(eBuffBonusType type)
    {
        return m_PropertyIndexers[(int)type];
    }

    public IMultiplicativeProperties BuffBonusMultCategory => m_buffMultBonus;
    public virtual int GetBaseStat(eStat stat)
    {
        return m_charStat[stat - eStat._First];
    }
    public virtual void ChangeBaseStat(eStat stat, short amount)
    {
        m_charStat[stat - eStat._First] += amount;
    }
    
    public virtual eProperty GetResistTypeForDamage(eDamageType damageType)
    {
	    if ((int)damageType < m_damageTypeToResistBonusConversion.Length)
	    {
		    return m_damageTypeToResistBonusConversion[(int)damageType];
	    }
	    else
	    {
		    log.ErrorFormat("No resist found for damage type {0} on living {1}!", (int)damageType, Name);
		    return 0;
	    }
    }    
    public virtual int GetResist(eDamageType damageType)
    {
	    return GetModified(GetResistTypeForDamage(damageType));
    }    
    public virtual int GetResistBase(eDamageType damageType)
    {
	    return GetModifiedBase(GetResistTypeForDamage(damageType));
    }    
    public virtual int GetDamageResist(eProperty property)
    {
	    return SkillBase.GetRaceResist( m_race, (eResist)property );
    }
    
    protected eActiveWeaponSlot m_activeWeaponSlot;
    public virtual eActiveWeaponSlot ActiveWeaponSlot => m_activeWeaponSlot;
    protected byte m_visibleActiveWeaponSlots = 0xFF; // none by default
    public byte VisibleActiveWeaponSlots
    {
	    get => m_visibleActiveWeaponSlots;
	    set { m_visibleActiveWeaponSlots=value; }
    }

    public virtual double ChanceToFumble
    {
	    get
	    {
		    double chanceToFumble = GetModified(eProperty.FumbleChance);
		    chanceToFumble *= 0.001;

		    if (chanceToFumble > 0.99) chanceToFumble = 0.99;
		    if (chanceToFumble < 0) chanceToFumble = 0;

		    return chanceToFumble;
	    }
    }

    public virtual double ChanceToBeMissed
    {
	    get
	    {
		    double chanceToBeMissed = GetModified(eProperty.MissHit);
		    chanceToBeMissed *= 0.001;

		    if (chanceToBeMissed > 0.99) chanceToBeMissed = 0.99;
		    if (chanceToBeMissed < 0) chanceToBeMissed = 0;

		    return chanceToBeMissed;
	    }
    }
    
    public virtual bool IsStrafing
    {
	    get { return false; }
	    set { }
    }
    
    #endregion

    #region main/left hand weapon
    public virtual bool CanUseLefthandedWeapon
    {
	    get { return false; }
	    set { }
    }
    public virtual int CalculateLeftHandSwingCount()
    {
	    return 0;
    }
    public virtual double CalculateLeftHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
    {
	    return 1.0;
    }
    public virtual double CalculateMainHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
    {
	    return 1.0;
    }
    
	public virtual void SwitchWeapon(eActiveWeaponSlot slot)
	{
		if (Inventory == null)
			return;

		InventoryItem rightHandSlot = Inventory.GetItem(eInventorySlot.RightHandWeapon);
		InventoryItem leftHandSlot = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
		InventoryItem twoHandSlot = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
		InventoryItem distanceSlot = Inventory.GetItem(eInventorySlot.DistanceWeapon);

		// simple active slot logic:
		// 0=right hand, 1=left hand, 2=two-hand, 3=range, F=none
		int rightHand = (VisibleActiveWeaponSlots & 0x0F);
		int leftHand = (VisibleActiveWeaponSlots & 0xF0) >> 4;


		// set new active weapon slot
		switch (slot)
		{
			case eActiveWeaponSlot.Standard:
				{
					if (rightHandSlot == null)
						rightHand = 0xFF;
					else
						rightHand = 0x00;

					if (leftHandSlot == null)
						leftHand = 0xFF;
					else
						leftHand = 0x01;
				}
				break;

			case eActiveWeaponSlot.TwoHanded:
				{
					if (twoHandSlot != null && (twoHandSlot.Hand == 1 || this is GameNPC)) // 2h
					{
						rightHand = leftHand = 0x02;
						break;
					}

					// 1h weapon in 2h slot
					if (twoHandSlot == null)
						rightHand = 0xFF;
					else
						rightHand = 0x02;

					if (leftHandSlot == null)
						leftHand = 0xFF;
					else
						leftHand = 0x01;
				}
				break;

			case eActiveWeaponSlot.Distance:
				{
					leftHand = 0xFF; // cannot use left-handed weapons if ranged slot active

					if (distanceSlot == null)
						rightHand = 0xFF;
					else if (distanceSlot.Hand == 1 || this is GameNPC) // NPC equipment does not have hand so always assume 2 handed bow
						rightHand = leftHand = 0x03; // bows use 2 hands, throwing axes 1h
					else
						rightHand = 0x03;
				}
				break;
		}

		m_activeWeaponSlot = slot;

		// pack active weapon slots value back
		m_visibleActiveWeaponSlots = (byte)(((leftHand & 0x0F) << 4) | (rightHand & 0x0F));
	}
	#endregion

	#region interrupt
	private RegionAction InterruptTimer { get; set; }
	public virtual void StartInterruptTimer(AttackData attack, int duration)
	{
		if(attack != null)
			StartInterruptTimer(duration, attack.AttackType, attack.Attacker);
	}

	public virtual void StartInterruptTimer(int duration, AttackData.eAttackType attackType, GameLiving attacker)
	{
		if (!IsAlive || ObjectState != eObjectState.Active)
		{
			InterruptTime = 0;
			InterruptAction = 0;
			return;
		}
		if (InterruptTime < CurrentRegion.Time + duration)
			InterruptTime = CurrentRegion.Time + duration;

		if (CurrentSpellHandler != null)
			CurrentSpellHandler.CasterIsAttacked(attacker);
		
		if (AttackState && ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			OnInterruptTick(attacker, attackType);
	}

	protected long m_interruptTime = 0;
	public virtual long InterruptTime
	{
		get { return m_interruptTime; }
		set
		{
			if (CurrentRegion != null)
				InterruptAction = CurrentRegion.Time;
			m_interruptTime = value;
		}
	}

	protected long m_interruptAction = 0;
	public virtual long InterruptAction
	{
		get { return m_interruptAction; }
		set { m_interruptAction = value; }
	}

	/// <summary>
	/// Yields true if interrupt action is running on this living.
	/// </summary>
	public virtual bool IsBeingInterrupted
	{
		get { return (m_interruptTime > CurrentRegion.Time); }
	}

	/// <summary>
	/// Base chance this living can be interrupted
	/// </summary>
	public virtual int BaseInterruptChance
	{
		get { return 65; }
	}

	/// <summary>
	/// How long does an interrupt last?
	/// </summary>
	public virtual int SpellInterruptDuration
	{
		get { return ServerProperties.Properties.SPELL_INTERRUPT_DURATION; }
	}

	/// <summary>
	/// The amount of time the caster has to wait before being able to cast again
	/// </summary>
	public virtual int SpellInterruptRecastTime
	{
		get { return ServerProperties.Properties.SPELL_INTERRUPT_RECAST; }
	}

	/// <summary>
	/// Additional interrupt time if interrupted again
	/// </summary>
	public virtual int SpellInterruptRecastAgain
	{
		get { return ServerProperties.Properties.SPELL_INTERRUPT_AGAIN; }
	}

	public virtual bool ChanceSpellInterrupt(GameLiving attacker)
	{
		double mod = GetConLevel(attacker);
		double chance = BaseInterruptChance;
		chance += mod * 10;
		chance = Math.Max(1, chance);
		chance = Math.Min(99, chance);
		if (attacker is GamePlayer) chance = 99;
		return RandomUtil.Chance((int)chance);
	}	
	protected virtual bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
	{
		return false;
	}	
    #endregion
    
	#region Abilities
	protected readonly Dictionary<string, Ability> m_abilities = new Dictionary<string, Ability>();
	protected readonly Object m_lockAbilities = new Object();

	public virtual bool HasAbility(string keyName)
	{
		bool hasit = false;
		
		lock (m_lockAbilities)
		{
			hasit = m_abilities.ContainsKey(keyName);
		}
		
		return hasit;
	}
	public virtual void AddAbility(Ability ability)
	{
		AddAbility(ability, true);
	}
	public virtual void AddAbility(Ability ability, bool sendUpdates)
	{
		bool isNewAbility = false;
		lock (m_lockAbilities)
		{
			Ability oldAbility = null;
			m_abilities.TryGetValue(ability.KeyName, out oldAbility);
			
			if (oldAbility == null)
			{
				isNewAbility = true;
				m_abilities.Add(ability.KeyName, ability);
				ability.Activate(this, sendUpdates);
			}
			else
			{
				int oldLevel = oldAbility.Level;
				oldAbility.Level = ability.Level;
				
				isNewAbility |= oldAbility.Level > oldLevel;
			}
			
			if (sendUpdates && (isNewAbility && (this is GamePlayer)))
			{
				(this as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this as GamePlayer).Network.Account.Language, "GamePlayer.AddAbility.YouLearn", ability.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}
	}
	public virtual bool RemoveAbility(string abilityKeyName)
	{
		Ability ability = null;
		lock (m_lockAbilities)
		{
			m_abilities.TryGetValue(abilityKeyName, out ability);
			
			if (ability == null)
				return false;
			
			ability.Deactivate(this, true);
			m_abilities.Remove(ability.KeyName);
		}
		
		if (this is GamePlayer)
			(this as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this as GamePlayer).Network.Account.Language, "GamePlayer.RemoveAbility.YouLose", ability.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		return true;
	}

	public Ability GetAbility(string abilityKey)
	{
		Ability ab = null;
		lock (m_lockAbilities)
		{
			m_abilities.TryGetValue(abilityKey, out ab);
		}
		
		return ab;
	}

	public T GetAbility<T>() where T : Ability
	{
		T tmp;
		lock (m_lockAbilities)
		{
			tmp = (T)m_abilities.Values.FirstOrDefault(a => a.GetType().Equals(typeof(T)));
		}
		
		return tmp;
	}
	
	public int GetAbilityLevel(string keyName)
	{
		Ability ab = null;
		
		lock (m_lockAbilities)
		{
			m_abilities.TryGetValue(keyName, out ab);
		}
		
		if (ab == null)
			return 0;

		return Math.Max(1, ab.Level);
	}

	public IList GetAllAbilities()
	{
		List<Ability> list = new List<Ability>();
		lock (m_lockAbilities)
		{
			list = new List<Ability>(m_abilities.Values);
		}
		
		return list;
	}

	public virtual bool HasAbilityToUseItem(ItemTemplate item)
	{
		return GameServer.ServerRules.CheckAbilityToUseItem(this, item);
	}
	#endregion
	
    #region Attacker
    protected AttackAction m_attackAction;
    protected readonly List<GameObject> m_attackers;
    public List<GameObject> Attackers => m_attackers;
    protected virtual AttackAction CreateAttackAction() => m_attackAction ?? new AttackAction(this);
	public virtual void AddAttacker(GameObject attacker)
	{
		lock (Attackers)
		{
			if (attacker == this) return;
			if (m_attackers.Contains(attacker)) return;
			m_attackers.Add(attacker);
		}
	}
	public virtual void RemoveAttacker(GameObject attacker)
	{
		lock (Attackers)
		{
			m_attackers.Remove(attacker);
        }
	}

	protected long m_lastAttackedByEnemyTick;
	protected long m_lastAttackedTick;
	public virtual long LastAttackedTick
	{
		get
		{
			return m_lastAttackedTick;
		}
		set
		{
			m_lastAttackedTick = value;
			if (this is GameNPC npc)
			{
				if (npc.Brain is IControlledBrain controlledBrain)
				{
					controlledBrain.Owner.m_lastAttackedTick = value;
				}
			}			
		}
	}
	public virtual long LastAttackedByEnemyTick
	{
		get
		{
			return m_lastAttackedByEnemyTick;
		}
		set
		{
			m_lastAttackedByEnemyTick = value;
			if (this is GameNPC npc)
			{
				if (npc.Brain is IControlledBrain controlledBrain)
				{
					controlledBrain.Owner.LastAttackedByEnemyTick = value;
				}
			}
		}
	}

	public long LastCombatTick
	{
		get
		{
			return m_lastAttackedByEnemyTick > m_lastAttackedTick ? m_lastAttackedByEnemyTick : m_lastAttackedTick;
		}
	}
	public virtual bool InCombat
	{
		get
		{
			bool ret = true;
			Region region = CurrentRegion;
			if (region == null || LastCombatTick == 0)
			{
				ret = false;
			}
			else
			{
				ret = LastCombatTick + 10000 >= region.Time;
			}
			
			if (ret == false)
			{
				if (Attackers.Count > 0)
				{
					Attackers.Clear();
				}
			}

			return ret;
		}
	}
	public virtual bool InCombatInLast(int milliseconds)
	{
		bool ret = true;
		Region region = CurrentRegion;
		if (region == null || LastCombatTick == 0)
		{
			ret = false;
		}
		else
		{
			ret = LastCombatTick + milliseconds >= region.Time;
		}
			
		if (ret == false)
		{
			if (Attackers.Count > 0)
			{
				Attackers.Clear();
			}
		}

		return ret;
	}
	
	public virtual void Die(GameObject killer)
	{
		if (this is GameNPC == false && this is GamePlayer == false)
		{
			// deal out exp and realm points based on server rules
			GameServer.ServerRules.OnLivingKilled(this, killer);
		}

		StopAttack();

		List<GameObject> clone;
		lock (Attackers)
		{
			if (m_attackers.Contains(killer) == false)
				m_attackers.Add(killer);
			clone = new List<GameObject>(m_attackers);
		}
		List<GamePlayer> playerAttackers = null;

		foreach (GameObject obj in clone)
		{
			if (obj is GameLiving)
			{
				GamePlayer player = obj as GamePlayer;

				if (obj as GameNPC != null && (obj as GameNPC).Brain is IControlledBrain)
				{
					// Ok, we're a pet - if our Player owner isn't in the attacker list, let's make them a 'virtual' attacker
					player = ((obj as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
					if (player != null)
					{
						if (clone.Contains(player) == false)
						{
							if (playerAttackers == null)
								playerAttackers = new List<GamePlayer>();

							if (playerAttackers.Contains(player) == false)
								playerAttackers.Add(player);
						}

						// Pet gets the killed message as well
						((GameLiving)obj).EnemyKilled(this);
					}
				}

				if (player != null)
				{
					if (playerAttackers == null)
						playerAttackers = new List<GamePlayer>();

					if (playerAttackers.Contains(player) == false)
					{
						playerAttackers.Add(player);
					}
				}
				else
				{
					((GameLiving)obj).EnemyKilled(this);
				}
			}
		}

		if (playerAttackers != null)
		{
			foreach (GamePlayer player in playerAttackers)
			{
				player.EnemyKilled(this);
			}
		}

		m_attackers.Clear();

		TargetObject = null;

		// cancel all left effects
		EffectList.CancelAll();

		// Stop the regeneration timers
		StopHealthRegeneration();
		StopPowerRegeneration();
		StopEnduranceRegeneration();

		//Reduce health to zero
		Health = 0;

		// Remove all last attacked times
		m_lastAttackedTick = 0;

		//Let's send the notification at the end
		Notify(GameLivingEvent.Dying, this, new DyingEventArgs(killer));
	}    
    #endregion
    
	#region Spell Cast
	public virtual double Effectiveness
	{
		get { return 1.0; }
		set { }
	}
	public virtual bool IsCasting
	{
		get { return m_runningSpellHandler != null && m_runningSpellHandler.IsCasting; }
	}
	public override bool HasEffect(Spell spell)
	{
		lock (EffectList)
		{
			foreach (IGameEffect effect in EffectList)
			{
				if (effect is GameSpellEffect)
				{
					GameSpellEffect spellEffect = effect as GameSpellEffect;

					if (spellEffect.Spell.SpellType == spell.SpellType &&
					    spellEffect.Spell.EffectGroup == spell.EffectGroup)
						return true;
				}
			}
		}

		return base.HasEffect(spell);
	}
	public override bool HasEffect(Type effectType)
	{
		lock (EffectList)
		{
			foreach (IGameEffect effect in EffectList)
				if (effect.GetType() == effectType)
					return true;
		}

		return base.HasEffect(effectType);
	}

	protected ISpellHandler m_runningSpellHandler;
	public ISpellHandler CurrentSpellHandler
	{
		get { return m_runningSpellHandler; }
		set { m_runningSpellHandler = value; }
	}
	public virtual void OnAfterSpellCastSequence(ISpellHandler handler)
	{
		m_runningSpellHandler.CastingCompleteEvent -= new CastingCompleteCallback(OnAfterSpellCastSequence);
		m_runningSpellHandler = null;
	}
	public virtual void StopCurrentSpellcast()
	{
		if (m_runningSpellHandler != null)
			m_runningSpellHandler.InterruptCasting();
	}
	public virtual bool CastSpell(Spell spell, SpellLine line)
	{
		if (IsStunned || IsMezzed)
		{
			Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.CrowdControlled));
			return false;
		}

		if ((m_runningSpellHandler != null && spell.CastTime > 0))
		{
			Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.AlreadyCasting));
			return false;
		}

		ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
		if (spellhandler != null)
		{
			if (spell.CastTime > 0)
			{
				m_runningSpellHandler = spellhandler;
				spellhandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
			}
			return spellhandler.CastSpell();
		}
		else
		{
			if (log.IsWarnEnabled)
				log.Warn(Name + " wants to cast but spell " + spell.Name + " not implemented yet");
		}

		return false;
	}

	public virtual bool CastSpell(ISpellCastingAbilityHandler ab)
	{
		ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, ab.Spell, ab.SpellLine);
		if (spellhandler != null)
		{
			// Instant cast abilities should not interfere with the spell queue
			if (spellhandler.Spell.CastTime > 0)
			{
				m_runningSpellHandler = spellhandler;
				m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
			}

			spellhandler.Ability = ab;
			return spellhandler.CastSpell();
		}
		return false;
	}

	#endregion
	
	#region Skills
	private readonly Dictionary<KeyValuePair<int, Type>, KeyValuePair<long, Skill>> m_disabledSkills = new Dictionary<KeyValuePair<int, Type>, KeyValuePair<long, Skill>>();
	public virtual int GetSkillDisabledDuration(Skill skill)
	{
		lock ((m_disabledSkills as ICollection).SyncRoot)
		{
			KeyValuePair<int, Type> key = new KeyValuePair<int, Type>(skill.ID, skill.GetType());
			if (m_disabledSkills.ContainsKey(key))
			{
				long timeout = m_disabledSkills[key].Key;
				long left = timeout - CurrentRegion.Time;
				if (left <= 0)
				{
					left = 0;
					m_disabledSkills.Remove(key);
				}
				return (int)left;
			}
		}
		return 0;
	}
	public virtual ICollection<Skill> GetAllDisabledSkills()
	{
		lock ((m_disabledSkills as ICollection).SyncRoot)
		{
			List<Skill> skillList = new List<Skill>();
			
			foreach(KeyValuePair<long, Skill> disabled in m_disabledSkills.Values)
				skillList.Add(disabled.Value);
			
			return skillList;
		}
	}
	public virtual void DisableSkill(Skill skill, int duration)
	{
		lock ((m_disabledSkills as ICollection).SyncRoot)
		{
			KeyValuePair<int, Type> key = new KeyValuePair<int, Type>(skill.ID, skill.GetType());
			if (duration > 0)
			{
				m_disabledSkills[key] = new KeyValuePair<long, Skill>(CurrentRegion.Time + duration, skill);
			}
			else
			{
				m_disabledSkills.Remove(key);
			}
		}
	}
	public virtual void DisableSkill(ICollection<Tuple<Skill, int>> skills)
	{
		lock ((m_disabledSkills as ICollection).SyncRoot)
		{
			foreach (Tuple<Skill, int> tuple in skills)
			{
				Skill skill = tuple.Item1;
				int duration = tuple.Item2;
				
				KeyValuePair<int, Type> key = new KeyValuePair<int, Type>(skill.ID, skill.GetType());
				if (duration > 0)
				{
					m_disabledSkills[key] = new KeyValuePair<long, Skill>(CurrentRegion.Time + duration, skill);
				}
				else
				{
					m_disabledSkills.Remove(key);
				}
			}
		}
	}
	public virtual void RemoveDisabledSkill(Skill skill)
	{
		lock ((m_disabledSkills as ICollection).SyncRoot)
		{
			KeyValuePair<int, Type> key = new KeyValuePair<int, Type>(skill.ID, skill.GetType());
			if(m_disabledSkills.ContainsKey(key))
				m_disabledSkills.Remove(key);
		}
	}
	#endregion
	
    #region Attack action
    public virtual double AttackDamage(InventoryItem weapon)
    {
	    double effectiveness = 1.00;
	    //double effectiveness = Effectiveness;
	    double damage = (1.0 + Level / 3.7 + Level * Level / 175.0) * AttackSpeed(weapon) * 0.001;
	    if (weapon == null || weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type == Slot.TWOHAND)
	    {
		    //Melee damage buff,debuff,RA
		    effectiveness += GetModified(eProperty.MeleeDamage) * 0.01;
	    }
	    else if (weapon.Item_Type == Slot.RANGED && (weapon.Object_Type == (int)eObjectType.Longbow || weapon.Object_Type == (int)eObjectType.RecurvedBow || weapon.Object_Type == (int)eObjectType.CompositeBow))
	    {
		    effectiveness += GetModified(eProperty.SpellDamage) * 0.01;
	    }
	    else if (weapon.Item_Type == Slot.RANGED)
	    {
		    effectiveness += GetModified(eProperty.RangedDamage) * 0.01;
	    }
	    damage *= effectiveness;
	    return damage;
    }    
	public virtual double UnstyledDamageCap(InventoryItem weapon)
	{
		return AttackDamage(weapon) * (2.82 + 0.00009 * AttackSpeed(weapon));
	}
	public virtual double CastingSpeedReductionCap
	{
		get { return 0.4f; }
	}
	public virtual int MinimumCastingSpeed
	{
		get { return 500; }
	}
	public virtual bool CanCastInCombat(Spell spell)
	{
		return true;
	}
	public virtual double DexterityCastTimeReduction
	{
		get
		{
			int dex = GetModified(eProperty.Dexterity);
			if (dex < 60) return 1.0;
			else if (dex < 250) return 1.0 - (dex - 60) * 0.15 * 0.01;
			else return 1.0 - ((dex - 60) * 0.15 + (dex - 250) * 0.05) * 0.01;
		}
	}
	public virtual int AttackRange
	{
		get
		{
			if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				return Math.Max(32, (int)(2000.0 * GetModified(eProperty.ArcheryRange) * 0.01));
			}
			return 200;
		}
		set { }
	}
	public virtual int GetWeaponStat(InventoryItem weapon)
	{
		return GetModified(eProperty.Strength);
	}

	public virtual double GetArmorAF(eArmorSlot slot)
	{
		return GetModified(eProperty.ArmorFactor);
	}

	public virtual double GetArmorAbsorb(eArmorSlot slot)
	{
		double absorbBonus = GetModified(eProperty.ArmorAbsorption) / 100.0;

		double debuffBuffRatio = 2;

		double constitutionPerAbsorptionPercent = 4;
		double baseConstitutionPerAbsorptionPercent = 12; //kept for DB legacy reasons

		var basebuff = GetBuffBonus(eBuffBonusType.BaseBuff);
		var specbuff = GetBuffBonus(eBuffBonusType.SpecBuff);
		var debuff = GetBuffBonus(eBuffBonusType.Debuff);
		var specdebuff = GetBuffBonus(eBuffBonusType.SpecDebuff);

		var constitutionBuffBonus = basebuff[eProperty.Constitution] + specbuff[eProperty.Constitution];
		var constitutionDebuffMalus = Math.Abs(debuff[eProperty.Constitution] + specdebuff[eProperty.Constitution]);
		double constitutionAbsorb = 0;
		
		double baseConstitutionAbsorb = (GetBaseStat((eStat)eProperty.Constitution) - 60) / baseConstitutionPerAbsorptionPercent / 100.0;
		double consitutionBuffAbsorb = (constitutionBuffBonus - constitutionDebuffMalus * debuffBuffRatio) / constitutionPerAbsorptionPercent / 100;
		constitutionAbsorb += baseConstitutionAbsorb + consitutionBuffAbsorb;

		double afPerAbsorptionPercent = 6;
		double liveBaseAFcap = 150 * 1.25 * 1.25;
		double afBuffBonus = Math.Min(liveBaseAFcap, basebuff[eProperty.ArmorFactor] + specbuff[eProperty.ArmorFactor]);
		double afDebuffMalus = Math.Abs(debuff[eProperty.ArmorFactor] + specdebuff[eProperty.ArmorFactor]);
		double afBuffAbsorb = (afBuffBonus - afDebuffMalus * debuffBuffRatio) / afPerAbsorptionPercent / 100;

		double baseAbsorb = 0;
		if (Level >= 30) baseAbsorb = 0.27;
		else if (Level >= 20) baseAbsorb = 0.19;
		else if (Level >= 10) baseAbsorb = 0.10;

		double absorb = 1 - (1 - absorbBonus) * (1 - baseAbsorb) * (1 - constitutionAbsorb) * (1 - afBuffAbsorb);
		return absorb;
	}

	public virtual double GetWeaponSkill(InventoryItem weapon)
	{
		const double bs = 128.0 / 50.0;	
		return (int)((Level + 1) * bs * (1 + (GetWeaponStat(weapon) - 50) * 0.005) * Level * 2 / 50);
	}
		
    public virtual InventoryItem AttackWeapon
    {
	    get
	    {
		    if (Inventory != null)
		    {
			    switch (ActiveWeaponSlot)
			    {
				    case eActiveWeaponSlot.Standard: return Inventory.GetItem(eInventorySlot.RightHandWeapon);
				    case eActiveWeaponSlot.TwoHanded: return Inventory.GetItem(eInventorySlot.TwoHandWeapon);
				    case eActiveWeaponSlot.Distance: return Inventory.GetItem(eInventorySlot.DistanceWeapon);
			    }
		    }
		    return null;
	    }
    }
    
    protected virtual Style GetStyleToUse()
    {
	    InventoryItem weapon;
	    if (NextCombatStyle == null) return null;
	    if (NextCombatStyle.WeaponTypeRequirement == (int)eObjectType.Shield)
		    weapon = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
	    else weapon = AttackWeapon;

	    if (StyleProcessor.CanUseStyle(this, NextCombatStyle, weapon))
		    return NextCombatStyle;

	    if (NextCombatBackupStyle == null) return NextCombatStyle;

	    return NextCombatBackupStyle;
    }

    protected Style m_nextCombatStyle;
    protected Style m_nextCombatBackupStyle;
    public Style NextCombatStyle
    {
	    get { return m_nextCombatStyle; }
	    set { m_nextCombatStyle = value; }
    }
    public Style NextCombatBackupStyle
    {
	    get { return m_nextCombatBackupStyle; }
	    set { m_nextCombatBackupStyle = value; }
    }
    public virtual int AttackSpeed(params InventoryItem[] weapon)
    {
	    double speed = 3000 * (1.0 - (GetModified(eProperty.Quickness) - 60) / 500.0);
	    if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
	    {
		    speed *= 1.5; // mob archer speed too fast
		    speed *= 1.0 - GetModified(eProperty.CastingSpeed) * 0.01;
	    }
	    else
	    {
		    speed *= GetModified(eProperty.MeleeSpeed) * 0.01;
	    }
	    return (int) Math.Max(500.0, speed);
    }
    public virtual int AttackCriticalChance(InventoryItem weapon) => 0;
	public virtual int SpellCriticalChance
	{
		get { return GetModified(eProperty.CriticalSpellHitChance); }
		set { }
	}
	public virtual eDamageType AttackDamageType(InventoryItem weapon) => eDamageType.Natural;
	public virtual bool AttackState { get; protected set; }
	public override bool IsAttackable => (IsAlive && !IsStealthed && ObjectState == GameObject.eObjectState.Active);
	public virtual bool IsAttacking => (AttackState && (m_attackAction != null) && m_attackAction.IsAlive);
	public virtual int EffectiveOverallAF => 0;
	public virtual int WeaponSpecLevel(InventoryItem weapon) => 0;
	public virtual double WeaponDamage(InventoryItem weapon) => 0;
	public virtual bool IsCrowdControlled => (IsStunned || IsMezzed);
	public virtual bool IsIncapacitated => (ObjectState != eObjectState.Active || !IsAlive || IsStunned || IsMezzed);
		
    public virtual void EnemyKilled(GameLiving enemy)
    {
	    RemoveAttacker(enemy);
	    Notify(GameLivingEvent.EnemyKilled, this, new EnemyKilledEventArgs(enemy));
    }
    
    public virtual void OnTargetDeadOrNoTarget()
    {
	    StopAttack();
    }

    /// <summary>
    /// Stops all attacks this GameLiving is currently making.
    /// </summary>
    public virtual void StopAttack()
    {
	    StopAttack(true);
    }

    /// <summary>
    /// Stop all attackes this GameLiving is currently making
    /// </summary>
    /// <param name="forced">Is this a forced stop or is the client suggesting we stop?</param>
    public virtual void StopAttack(bool forced)
    {
	    AttackState = false;
    }
    #endregion

    #region attack calc
	protected virtual float MinMeleeCriticalDamage
	{
		get { return 0.1f; }
	}
	public virtual int GetMeleeCriticalDamage(AttackData attackData, InventoryItem weapon)
	{
		if (RandomUtil.Chance(AttackCriticalChance(weapon)))
		{
			int maxCriticalDamage = (attackData.Target is GamePlayer)
				? attackData.Damage / 2
				: attackData.Damage;

			int minCriticalDamage = (int)(attackData.Damage * MinMeleeCriticalDamage);

			return RandomUtil.Int(minCriticalDamage, maxCriticalDamage);
		}
		return 0;
	}
	protected bool IsValidTarget
	{
		get
		{
			return true; //EffectList.CountOfType<NecromancerShadeEffect>() <= 0;
		}
	}
	public GamePlayer GetPlayerAttacker(GameLiving living)
	{
		if (living is GamePlayer)
			return living as GamePlayer;

		GameNPC npc = living as GameNPC;

		if (npc != null)
		{
			if (npc.Brain is IControlledBrain && (npc.Brain as IControlledBrain).Owner is GamePlayer)
				return (npc.Brain as IControlledBrain).Owner as GamePlayer;
		}

		return null;
	}
		public virtual eAttackResult CalculateEnemyAttackResult(AttackData ad, InventoryItem weapon)
		{
			if (!IsValidTarget)
				return eAttackResult.NoValidTarget;

			GameSpellEffect bladeturn = null;

			// ML effects
			GameSpellEffect phaseshift = null;
			GameSpellEffect grapple = null;
			GameSpellEffect brittleguard = null;

			AttackData lastAD = TempProperties.getProperty<AttackData>(LAST_ATTACK_DATA, null);
			bool defenseDisabled = ad.Target.IsMezzed | ad.Target.IsStunned | ad.Target.IsSitting;

			// get all needed effects in one loop
			lock (EffectList)
			{
				foreach (IGameEffect effect in EffectList)
				{
					if (effect is GameSpellEffect)
					{
						switch ((effect as GameSpellEffect).Spell.SpellType)
						{
							case "Phaseshift":
								if (phaseshift == null)
									phaseshift = (GameSpellEffect)effect;
								continue;
							case "Grapple":
								if (grapple == null)
									grapple = (GameSpellEffect)effect;
								continue;
							case "BrittleGuard":
								if (brittleguard == null)
									brittleguard = (GameSpellEffect)effect;
								continue;
							case "Bladeturn":
								if (bladeturn == null)
									bladeturn = (GameSpellEffect)effect;
								continue;
						}
					}
				}
			}

			bool stealthStyle = false;
			if (ad.Style != null && ad.Style.StealthRequirement && ad.Attacker is GamePlayer && StyleProcessor.CanUseStyle((GamePlayer)ad.Attacker, ad.Style, weapon))
			{
				stealthStyle = true;
				defenseDisabled = true;
				brittleguard = null;
			}
			
			if (phaseshift != null)
				return eAttackResult.Missed;

			if (grapple != null)
				return eAttackResult.Grappled;

			if (brittleguard != null)
			{
				if (this is GamePlayer)
					((GamePlayer)this).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)this).Network.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowIntercepted"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				if (ad.Attacker is GamePlayer)
					((GamePlayer)ad.Attacker).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Attacker).Network.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeIntercepted"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				brittleguard.Cancel(false);
				return eAttackResult.Missed;
			}

			double attackerConLevel = -GetConLevel(ad.Attacker);
			int attackerCount = m_attackers.Count;

			if (!defenseDisabled)
			{
				double evadeChance = TryEvade( ad, lastAD, attackerConLevel, attackerCount );

				if( RandomUtil.Chance( evadeChance ) )
					return eAttackResult.Evaded;

				if( ad.IsMeleeAttack )
				{
					double parryChance = TryParry( ad, lastAD, attackerConLevel, attackerCount );

					if( RandomUtil.Chance( parryChance ) )
						return eAttackResult.Parried;
				}

				double blockChance = TryBlock( ad, lastAD, attackerConLevel, attackerCount );

				if( RandomUtil.Chance( blockChance ) )
				{
					// reactive effects on block moved to GamePlayer
					return eAttackResult.Blocked;
				}
			}
			
			// Missrate
			int missrate = (ad.Attacker is GamePlayer) ? 20 : 25; //player vs player tests show 20% miss on any level
			missrate -= ad.Attacker.GetModified(eProperty.ToHitBonus);
			
			if (this is GameNPC || ad.Attacker is GameNPC) // if target is not player use level mod
			{
				missrate += (int)(5 * ad.Attacker.GetConLevel(this));
			}

			// weapon/armor bonus
			int armorBonus = 0;
			if (ad.Target is GamePlayer)
			{
				ad.ArmorHitLocation = ((GamePlayer)ad.Target).CalculateArmorHitLocation(ad);
				InventoryItem armor = null;
				if (ad.Target.Inventory != null)
					armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);
				if (armor != null)
					armorBonus = armor.Bonus;
			}
			if (weapon != null)
			{
				armorBonus -= weapon.Bonus;
			}
			if (ad.Target is GamePlayer && ad.Attacker is GamePlayer)
			{
				missrate += armorBonus;
			}
			else
			{
				missrate += missrate * armorBonus / 100;
			}
			if (ad.Style != null)
			{
				missrate -= ad.Style.BonusToHit; // add style bonus
			}
			if (lastAD != null && lastAD.AttackResult == eAttackResult.HitStyle && lastAD.Style != null)
			{
				// add defence bonus from last executed style if any
				missrate += lastAD.Style.BonusToDefense;
			}
			if (this is GamePlayer && ad.Attacker is GamePlayer && weapon != null)
			{
				missrate -= (int)((ad.Attacker.WeaponSpecLevel(weapon) - 1) * 0.1);
			}
			if (this is GamePlayer && ((GamePlayer)this).IsSitting)
			{
				missrate >>= 1; //halved
			}

			if (RandomUtil.Chance(missrate))
			{
				return eAttackResult.Missed;
			}

			if (ad.IsRandomFumble)
				return eAttackResult.Fumbled;

			if (ad.IsRandomMiss)
				return eAttackResult.Missed;


			// Bladeturn
			if (bladeturn != null)
			{
				bool penetrate = false;

				if (stealthStyle)
					penetrate = true;

				if (ad.AttackType == AttackData.eAttackType.Ranged && ad.Target != bladeturn.SpellHandler.Caster && ad.Attacker is GamePlayer && ((GamePlayer)ad.Attacker).HasAbility(Abilities.PenetratingArrow))
					penetrate = true;


				if (ad.IsMeleeAttack && !RandomUtil.Chance((double)bladeturn.SpellHandler.Caster.Level / (double)ad.Attacker.Level))
					penetrate = true;
				if (penetrate)
				{
					if (ad.Target is GamePlayer) ((GamePlayer)ad.Target).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Network.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowPenetrated"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					bladeturn.Cancel(false);
				}
				else
				{
					if (this is GamePlayer) ((GamePlayer)this).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)this).Network.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					if (ad.Attacker is GamePlayer) ((GamePlayer)ad.Attacker).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Attacker).Network.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					bladeturn.Cancel(false);
					Stealth(false);
					return eAttackResult.Missed;
				}
			}
			
			return eAttackResult.HitUnstyled;
		}

		protected virtual double TryEvade( AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount )
		{
			double evadeChance = 0;
			GamePlayer player = this as GamePlayer;

			GameSpellEffect evadeBuff = SpellHandler.FindEffectOnTarget( this, "EvadeBuff" );
			if( evadeBuff == null )
				evadeBuff = SpellHandler.FindEffectOnTarget( this, "SavageEvadeBuff" );

			if( player != null )
			{
				if (player.HasAbility(Abilities.Advanced_Evade) )
					evadeChance = GetModified( eProperty.EvadeChance );
				else if( IsObjectInFront( ad.Attacker, 180 ) && ( evadeBuff != null || player.HasAbility( Abilities.Evade ) ) )
				{
					int res = GetModified( eProperty.EvadeChance );
					if( res > 0 )
						evadeChance = res;
				}
			}
			else if( this is GameNPC && IsObjectInFront( ad.Attacker, 180 ) )
				evadeChance = GetModified( eProperty.EvadeChance );

			if( evadeChance > 0 && !ad.Target.IsStunned && !ad.Target.IsSitting )
			{
				if( attackerCount > 1 )
					evadeChance -= ( attackerCount - 1 ) * 0.03;

				evadeChance *= 0.001;
				evadeChance += 0.01 * attackerConLevel; // 1% per con level distance multiplied by evade level

				if( lastAD != null && lastAD.Style != null )
				{
					evadeChance += lastAD.Style.BonusToDefense * 0.01;
				}

				if( ad.AttackType == AttackData.eAttackType.Ranged )
					evadeChance /= 5.0;

				if( evadeChance < 0.01 )
					evadeChance = 0.01;
				else if( evadeChance > ServerProperties.Properties.EVADE_CAP && ad.Attacker is GamePlayer && ad.Target is GamePlayer )
					evadeChance = ServerProperties.Properties.EVADE_CAP; //50% evade cap RvR only; http://www.camelotherald.com/more/664.shtml
				else if( evadeChance > 0.995 )
					evadeChance = 0.995;
			}
			if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
			{
				evadeChance = Math.Max(evadeChance - 0.25, 0);
			}

			return evadeChance;
		}

		protected virtual double TryParry( AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount )
		{
			double parryChance = 0;

			if( ad.IsMeleeAttack )
			{
				GamePlayer player = this as GamePlayer;

				GameSpellEffect parryBuff = SpellHandler.FindEffectOnTarget( this, "ParryBuff" );
				if( parryBuff == null )
					parryBuff = SpellHandler.FindEffectOnTarget( this, "SavageParryBuff" );

				if( player != null )
				{
					if( IsObjectInFront( ad.Attacker, 120 ) )
					{
						if( ( player.HasSpecialization( Specs.Parry ) || parryBuff != null ) && ( AttackWeapon != null ) )
							parryChance = GetModified( eProperty.ParryChance );
					}
				}
				else if( this is GameNPC && IsObjectInFront( ad.Attacker, 120 ) )
					parryChance = GetModified( eProperty.ParryChance );

				if( parryChance > 0 && !ad.Target.IsStunned && !ad.Target.IsSitting )
				{
					if( attackerCount > 1 )
						parryChance /= attackerCount / 2;

					parryChance *= 0.001;
					parryChance += 0.05 * attackerConLevel;

					if( parryChance < 0.01 )
						parryChance = 0.01;
					else if( parryChance > ServerProperties.Properties.PARRY_CAP && ad.Attacker is GamePlayer && ad.Target is GamePlayer )
						parryChance = ServerProperties.Properties.PARRY_CAP;
					else if( parryChance > 0.995 )
						parryChance = 0.995;
				}
			}
			return parryChance;
		}

		protected virtual double TryBlock( AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount)
		{
			double blockChance = 0;
			GamePlayer player = this as GamePlayer;
			InventoryItem lefthand = null;

			if( this is GamePlayer && player != null && IsObjectInFront( ad.Attacker, 120 ) && player.HasAbility( Abilities.Shield ) )
			{
				lefthand = Inventory.GetItem( eInventorySlot.LeftHandWeapon );
				if( lefthand != null && ( player.AttackWeapon == null || player.AttackWeapon.Item_Type == Slot.RIGHTHAND || player.AttackWeapon.Item_Type == Slot.LEFTHAND ) )
				{
					if( lefthand.Object_Type == (int)eObjectType.Shield && IsObjectInFront( ad.Attacker, 120 ) )
						blockChance = GetModified( eProperty.BlockChance ) * lefthand.Quality * 0.01;
				}
			}
			else if( this is GameNPC && IsObjectInFront( ad.Attacker, 120 ) )
			{
				int res = GetModified( eProperty.BlockChance );
				if( res != 0 )
					blockChance = res;
			}
			if( blockChance > 0 && IsObjectInFront( ad.Attacker, 120 ) && !ad.Target.IsStunned && !ad.Target.IsSitting )
			{
				// Reduce block chance if the shield used is too small (valable only for player because npc inventory does not store the shield size but only the model of item)
				int shieldSize = 0;
				if( lefthand != null )
					shieldSize = lefthand.Type_Damage;
				if( player != null && attackerCount > shieldSize )
					blockChance *= (shieldSize / attackerCount);

				blockChance *= 0.001;
				blockChance += attackerConLevel * 0.05;

				if (blockChance < 0.01)
					blockChance = 0.01;
				else if (blockChance > ServerProperties.Properties.BLOCK_CAP && ad.Attacker is GamePlayer)
					blockChance = ServerProperties.Properties.BLOCK_CAP;
				else if (shieldSize == 1 && ad.Attacker is GameNPC && blockChance > .8)
					blockChance = .8;
				else if (shieldSize == 2 && ad.Attacker is GameNPC && blockChance > .9)
					blockChance = .9;
				else if (shieldSize == 3 && ad.Attacker is GameNPC && blockChance > .99)
					blockChance = .99;
			}
			if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
			{
				blockChance = Math.Max(blockChance - 0.25, 0);
			}

			return blockChance;
		}
		
		public virtual void ModifyAttack(AttackData attackData)
		{
		}

		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			base.TakeDamage(source, damageType, damageAmount, criticalAmount);

			double damageDealt = damageAmount + criticalAmount;

			if (source != null && source is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)source).Brain as IControlledBrain;
				if (brain != null)
					source = brain.GetLivingOwner();
			}

			bool wasAlive = IsAlive;
			
			Health -= damageAmount + criticalAmount;

			Stealth(false);

			if (!IsAlive)
			{
				if (wasAlive)
					Die(source);
			}
			else
			{
				if (IsLowHealth)
					Notify(GameLivingEvent.LowHealth, this, null);
			}
		}

		public virtual void OnAttackedByEnemy(AttackData ad)
		{
			if (ad.IsHit && ad.CausesCombat)
			{
				Notify(GameLivingEvent.AttackedByEnemy, this, new AttackedByEnemyEventArgs(ad));

				if (this is GameNPC && ActiveWeaponSlot == eActiveWeaponSlot.Distance && this.IsWithinRadius(ad.Attacker, 150))
					((GameNPC)this).SwitchToMelee(ad.Attacker);

				AddAttacker( ad.Attacker );

				if (ad.Attacker.Realm == 0 || this.Realm == 0)
				{
					LastAttackedByEnemyTick = CurrentRegion.Time;
					ad.Attacker.LastAttackedTick = CurrentRegion.Time;
				}
				else
				{
					LastAttackedByEnemyTick = CurrentRegion.Time;
					ad.Attacker.LastAttackedTick = CurrentRegion.Time;
				}
			}
		}

		/// <summary>
		/// Called to display an attack animation of this living
		/// </summary>
		/// <param name="ad">Infos about the attack</param>
		/// <param name="weapon">The weapon used for attack</param>
		public virtual void ShowAttackAnimation(AttackData ad, InventoryItem weapon)
		{
			bool showAnim = false;
			switch (ad.AttackResult)
			{
				case eAttackResult.HitUnstyled:
				case eAttackResult.HitStyle:
				case eAttackResult.Evaded:
				case eAttackResult.Parried:
				case eAttackResult.Missed:
				case eAttackResult.Blocked:
				case eAttackResult.Fumbled:
					showAnim = true; break;
			}

			if (showAnim && ad.Target != null)
			{
				//http://dolserver.sourceforge.net/forum/showthread.php?s=&threadid=836
				byte resultByte = 0;
				int attackersWeapon = (weapon == null) ? 0 : weapon.Model;
				int defendersWeapon = 0;

				switch (ad.AttackResult)
				{
						case eAttackResult.Missed: resultByte = 0; break;
						case eAttackResult.Evaded: resultByte = 3; break;
						case eAttackResult.Fumbled: resultByte = 4; break;
						case eAttackResult.HitUnstyled: resultByte = 10; break;
						case eAttackResult.HitStyle: resultByte = 11; break;

					case eAttackResult.Parried:
						resultByte = 1;
						if (ad.Target != null && ad.Target.AttackWeapon != null)
						{
							defendersWeapon = ad.Target.AttackWeapon.Model;
						}
						break;

					case eAttackResult.Blocked:
						resultByte = 2;
						if (ad.Target != null && ad.Target.Inventory != null)
						{
							InventoryItem lefthand = ad.Target.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
							if (lefthand != null && lefthand.Object_Type == (int)eObjectType.Shield)
							{
								defendersWeapon = lefthand.Model;
							}
						}
						break;
				}

				foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
				{
					if (player == null) continue;
					int animationId;
					switch (ad.AnimationId)
					{
						case -1:
							animationId = player.Out.OneDualWeaponHit;
							break;
						case -2:
							animationId = player.Out.BothDualWeaponHit;
							break;
						default:
							animationId = ad.AnimationId;
							break;
					}
					player.Out.SendCombatAnimation(this, ad.Target, (ushort)attackersWeapon, (ushort)defendersWeapon, animationId, 0, resultByte, ad.Target.HealthPercent);
				}
			}
		}

		/// <summary>
		/// This method is called whenever this living is dealing
		/// damage to some object
		/// </summary>
		/// <param name="ad">AttackData</param>
		public virtual void DealDamage(AttackData ad)
		{
			ad.Target.TakeDamage(ad);
		}
//====================================================================================================================
    #endregion
    
    #region Inventory
    protected IGameInventory m_inventory;
    public IGameInventory Inventory
    {
	    get
	    {
		    return m_inventory;
	    }
	    set
	    {
		    m_inventory = value;
	    }
    }
    #endregion
    
    #region Effects
    protected readonly GameEffectList m_effects;
    public GameEffectList EffectList => m_effects;
    protected virtual GameEffectList CreateEffectsList() => new GameEffectList(this);
    #endregion
    
    #region Stealth
    public virtual bool CanStealth
    { get; set; }
    
    public virtual bool IsStealthed
    {
	    get { return false; }
    }

    public virtual void Stealth(bool goStealth)
    {
	    if (goStealth)
		    log.Warn($"Stealth(): {GetType().FullName} cannot be stealthed.  You probably need to override Stealth() for this class");
    }
    #endregion
    
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
    
	#region Say/Yell/Whisper/Emote/Messages

		private bool m_isSilent = false;
		
		public virtual bool IsSilent
		{
			get { return m_isSilent; }
			set { m_isSilent = value; }
		}

		public virtual bool SayReceive(GameLiving source, string str)
		{
			if (source == null || str == null)
			{
				return false;
			}
			
			Notify(GameLivingEvent.SayReceive, this, new SayReceiveEventArgs(source, this, str));
			
			return true;
		}

		/// <summary>
		/// Broadcasts a message to all living beings around this object
		/// </summary>
		/// <param name="str">string to broadcast (without any "xxx says:" in front!!!)</param>
		/// <returns>true if text was said successfully</returns>
		public virtual bool Say(string str)
		{
			if (str == null || IsSilent)
			{
				return false;
			}
			
			Notify(GameLivingEvent.Say, this, new SayEventArgs(str));
			
			foreach (GameNPC npc in GetNPCsInRadius(WorldManager.SAY_DISTANCE))
			{
				GameNPC receiver = npc;
				// don't send say to the target, it will be whispered...
				if (receiver != this && receiver != TargetObject)
				{
					receiver.SayReceive(this, str);
				}
			}
			
			foreach (GamePlayer player in GetPlayersInRadius(WorldManager.SAY_DISTANCE))
			{
				GamePlayer receiver = player;
				if (receiver != this)
				{
					receiver.SayReceive(this, str);
				}
			}
			
			// whisper to Targeted NPC.
			if (TargetObject != null && TargetObject is GameNPC)
			{
				GameNPC targetNPC = (GameNPC)TargetObject;
				targetNPC.WhisperReceive(this, str);
			}
			
			return true;
		}

		/// <summary>
		/// This function is called when the living receives a yell
		/// </summary>
		/// <param name="source">GameLiving that was yelling</param>
		/// <param name="str">string that was yelled</param>
		/// <returns>true if the string should be processed further, false if it should be discarded</returns>
		public virtual bool YellReceive(GameLiving source, string str)
		{
			if (source == null || str == null)
			{
				return false;
			}
			
			Notify(GameLivingEvent.YellReceive, this, new YellReceiveEventArgs(source, this, str));
			
			return true;
		}

		/// <summary>
		/// Broadcasts a message to all living beings around this object
		/// </summary>
		/// <param name="str">string to broadcast (without any "xxx yells:" in front!!!)</param>
		/// <returns>true if text was yelled successfully</returns>
		public virtual bool Yell(string str)
		{
			if (str == null || IsSilent)
			{
				return false;
			}
			
			Notify(GameLivingEvent.Yell, this, new YellEventArgs(str));
			
			foreach (GameNPC npc in GetNPCsInRadius(WorldManager.YELL_DISTANCE))
			{
				GameNPC receiver = npc;
				if (receiver != this)
				{
					receiver.YellReceive(this, str);
				}
			}
			
			foreach (GamePlayer player in GetPlayersInRadius(WorldManager.YELL_DISTANCE))
			{
				GamePlayer receiver = player;
				if (receiver != this)
				{
					receiver.YellReceive(this, str);
				}
			}
			
			return true;
		}

		/// <summary>
		/// This function is called when the Living receives a whispered text
		/// </summary>
		/// <param name="source">GameLiving that was whispering</param>
		/// <param name="str">string that was whispered</param>
		/// <returns>true if the string should be processed further, false if it should be discarded</returns>
		public virtual bool WhisperReceive(GameLiving source, string str)
		{
			if (source == null || str == null)
			{
				return false;
			}

			GamePlayer player = null;
			if (source != null && source is GamePlayer)
			{
				player = source as GamePlayer;
				long whisperdelay = player.TempProperties.getProperty<long>("WHISPERDELAY");
				if (whisperdelay > 0 && (CurrentRegion.Time - 1500) < whisperdelay && player.Network.Account.PrivLevel == 1)
				{
					//player.Out.SendMessage("Speak slower!", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
					return false;
				}
				
				player.TempProperties.setProperty("WHISPERDELAY", CurrentRegion.Time);
			}

			Notify(GameLivingEvent.WhisperReceive, this, new WhisperReceiveEventArgs(source, this, str));

			return true;
		}

		/// <summary>
		/// Sends a whisper to a target
		/// </summary>
		/// <param name="target">The target of the whisper</param>
		/// <param name="str">text to whisper (without any "xxx whispers:" in front!!!)</param>
		/// <returns>true if text was whispered successfully</returns>
		public virtual bool Whisper(GameObject target, string str)
		{
			if (target == null || str == null || IsSilent)
			{
				return false;
			}
			
			if (!this.IsWithinRadius(target, WorldManager.WHISPER_DISTANCE))
			{
				return false;
			}
			
			Notify(GameLivingEvent.Whisper, this, new WhisperEventArgs(target, str));
			
			if (target is GameLiving)
			{
				return ((GameLiving)target).WhisperReceive(this, str);
			}
			
			return false;
		}
		/// <summary>
		/// Makes this living do an emote-animation
		/// </summary>
		/// <param name="emote">the emote animation to show</param>
		public virtual void Emote(eEmote emote)
		{
			foreach (GamePlayer player in GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
			{
				player.Out.SendEmoteAnimation(this, emote);
			}
		}

		/// <summary>
		/// A message to this living
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		public virtual void MessageToSelf(string message, eChatType chatType)
		{
			// livings can't talk to themselves
		}

		/// <summary>
		/// A message from something we control
		/// </summary>
		/// <param name="message"></param>
		/// <param name="chatType"></param>
		public virtual void MessageFromControlled(string message, eChatType chatType)
		{
			// ignore for livings
		}
		#endregion    
    public virtual bool TargetInView
    {
	    get => true;
	    set{}
    }

    public PropertyCollection TempProperties => m_tempProps;
    
    protected short m_race;
    public virtual short Race
    {
        get { return m_race; }
        set { m_race = value; }
    }    

    #region GetModifieds
    public override int EffectiveLevel
    {
	    get { return GetModified(eProperty.LivingEffectiveLevel); }
    }    
    public virtual int GetBaseSpecLevel(string keyName)
    {
	    return Level;
    }
    
    public virtual int GetModifiedSpecLevel(string keyName)
    {
	    return Level;
    }    
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
	
	protected virtual int PowerRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
	{
		if (this is GamePlayer )
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
			if (IsAlive && (m_mana < maxmana || (this is GamePlayer && ((GamePlayer)this).CharacterClass.ID > 0 && ((GamePlayer)this).CharacterClass.ID < 63)))
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
	
	public virtual int Concentration
	{
		get { return 0; }
	}

	public virtual int MaxConcentration
	{
		get { return 0; }
	}

	public virtual byte ConcentrationPercent
	{
		get
		{
			return (byte)(MaxConcentration <= 0 ? 0 : ((Concentration * 100) / MaxConcentration));
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

	private readonly ConcentrationList m_concEffects;
	public ConcentrationList ConcentrationEffects { get { return m_concEffects; } }
	public void CancelAllConcentrationEffects()
	{
		CancelAllConcentrationEffects(false);
	}

	public void CancelAllConcentrationEffects(bool leaveSelf)
	{
		// cancel conc spells
		ConcentrationEffects.CancelAll(leaveSelf);

		// cancel all active conc spell effects from other casters
		ArrayList concEffects = new ArrayList();
		lock (EffectList)
		{
			foreach (IGameEffect effect in EffectList)
			{
				if (effect is GameSpellEffect && ((GameSpellEffect)effect).Spell.Concentration > 0)
				{
					if (!leaveSelf || leaveSelf && ((GameSpellEffect)effect).SpellHandler.Caster != this)
						concEffects.Add(effect);
				}
			}
		}
		foreach (GameSpellEffect effect in concEffects)
		{
			effect.Cancel(false);
		}
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
				foreach (GamePlayer player in GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
				{
					if (player == null)
						continue;
					player.Network.Out.SendLivingDataUpdate(this, false);
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
		return base.MoveTo(position);
	}
	#endregion

	#region Crowd control
	protected bool m_stunned;
	public bool IsStunned
	{
		get { return m_stunned; }
		set { m_stunned = value; }
	}

	protected bool m_mezzed;
	public bool IsMezzed
	{
		get { return m_mezzed; }
		set { m_mezzed = value; }
	}
	protected bool m_disarmed = false;
	protected long m_disarmedTime = 0;
	public bool IsDisarmed
	{
		get { return (m_disarmedTime > 0 && m_disarmedTime > CurrentRegion.Time); }
	}
	public long DisarmedTime
	{
		get { return m_disarmedTime; }
		set { m_disarmedTime = value; }
	}

	protected bool m_isSilenced = false;
	protected long m_silencedTime = 0;
	public bool IsSilenced
	{
		get { return (m_silencedTime > 0 && m_silencedTime > CurrentRegion.Time); }
	}
	public long SilencedTime
	{
		get { return m_silencedTime; }
		set { m_silencedTime = value; }
	}
	
	protected volatile byte m_diseasedCount;
	public virtual void Disease(bool active)
	{
		if (active) m_diseasedCount++;
		else m_diseasedCount--;

		if (m_diseasedCount < 0)
		{
			if (log.IsErrorEnabled)
				log.Error("m_diseasedCount is less than zero.\n" + Environment.StackTrace);
		}
	}
	public virtual bool IsDiseased
	{
		get { return m_diseasedCount > 0; }
	}
	
	protected sbyte m_turningDisabledCount;
	public bool IsTurningDisabled
	{
		get { return m_turningDisabledCount > 0; }
	}
	public virtual void DisableTurning(bool add)
	{
		if (add) m_turningDisabledCount++;
		else m_turningDisabledCount--;

		if (m_turningDisabledCount < 0)
			m_turningDisabledCount=0;
	}	
	#endregion

	public virtual void BroadcastLivingEquipmentUpdate()
	{
		if (ObjectState != eObjectState.Active)
			return;
			
		foreach (GamePlayer player in GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
		{
			if (player == null)
				continue;
				
			player.Out.SendLivingEquipmentUpdate(this);
		}
	}
	
	public override void Notify(GameEvent e, object sender, EventArgs args)
	{
		if (e == GameLivingEvent.Interrupted && args != null)
		{
			if (CurrentSpellHandler != null)
				CurrentSpellHandler.CasterIsAttacked((args as InterruptedEventArgs).Attacker);

			return;
		}

		base.Notify(e, sender, args);
	}	
	
    /// <summary>
    /// 생성 초기화
    /// </summary>
    public GameLiving() : base()
    {
	    m_guildName = string.Empty;
	    
        for (eBuffBonusType i = 0; i < eBuffBonusType.MaxBonusType; i++)
        {
            m_PropertyIndexers[(int)i] = i switch
            {
                _=> new PropertyIndexer()
            };
        }
        
        m_targetObjectWeakReference = new WeakRef(null);
        m_activeWeaponSlot = eActiveWeaponSlot.Standard;
        m_attackers = new List<GameObject>();
        m_effects = CreateEffectsList();
        m_concEffects = new ConcentrationList(this);
        m_attackers = new List<GameObject>();
        
        m_health = 1;
        m_mana = 1;
        m_endurance = 1;
        m_maxEndurance = 1;
        m_lastAttackedTick = 0;
        
        CanStealth = false;
    }
}
using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Game.Logic.AI.Brain;
using Game.Logic.CharacterClasses;
using Game.Logic.Currencys;
using Game.Logic.Effects;
using Game.Logic.Events;
using Game.Logic.Geometry;
using Game.Logic.Guild;
using Game.Logic.Inventory;
using Game.Logic.Language;
using Game.Logic.network;
using Game.Logic.PropertyCalc;
using Game.Logic.RealmAblilities;
using Game.Logic.Skills;
using Game.Logic.Spells;
using Game.Logic.Styles;
using Game.Logic.Utils;
using Game.Logic.World;
using Game.Logic.World.Timer;
using log4net;
using Logic.database;
using Logic.database.table;
using NetworkMessage;
using Money = Game.Logic.Currencys.Money;

namespace Game.Logic
{
    public class GamePlayer : GameLiving
    {
	    public const int PLAYER_BASE_SPEED = 191;
        private GameClient mNetwork = null;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object m_LockObject = new object();
        private Wallet Wallet { get; }
        
        protected DOLCharacters mdbCharacter;
        
        internal DOLCharacters DBCharacter
        {
            get { return mdbCharacter; }
        }
        
        public GameClient Network
        {
            get => mNetwork;
            set
            {
                mNetwork = value;
            }
        }

        public OutPacket Out => Network.Out;
        
        public void AddMoney(Money money) => Wallet.AddMoney(money);
        public bool RemoveMoney(Money money) => Wallet.RemoveMoney(money);
        
        public string ObjectId
        {
	        get { return DBCharacter != null ? DBCharacter.ObjectId : InternalID; }
	        set { if (DBCharacter != null) DBCharacter.ObjectId = value; }
        }
        
        public virtual CharacterClass CharacterClass { get; protected set; }
        public string Salutation => CharacterClass.GetSalutation(Gender);
        public string AccountName => DBCharacter != null ? DBCharacter.AccountName : string.Empty;
        public DateTime CreationDate => DBCharacter != null ? DBCharacter.CreationDate : DateTime.MinValue;
        public DateTime LastPlayed => DBCharacter != null ? DBCharacter.LastPlayed : DateTime.MinValue;

        public long DeathTime
        {
	        get { return DBCharacter != null ? DBCharacter.DeathTime : 0; }
	        set { if (DBCharacter != null) DBCharacter.DeathTime = value; }
        }
        
        public byte DeathCount
        {
	        get { return DBCharacter != null ? DBCharacter.DeathCount : (byte)0; }
	        set { if (DBCharacter != null) DBCharacter.DeathCount = value; }
        }
        
        public override eGender Gender
        {
	        get
	        {
		        if (DBCharacter.Gender == 0)
		        {
			        return eGender.Male;
		        }

		        return eGender.Female;
	        }
	        set
	        {
	        }
        }
        
        public virtual string LastName
        {
	        get { return DBCharacter != null ? DBCharacter.LastName : string.Empty; }
	        set
	        {
		        if (DBCharacter == null) return;
		        DBCharacter.LastName = value;
		        //update last name for all players if client is playing
		        if (ObjectState == eObjectState.Active)
		        {
			        Out.SendUpdatePlayer();
			        foreach (GamePlayer player in GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
			        {
				        if (player == null) continue;
				        if (player != this)
				        {
					        player.Out.SendObjectRemove(this);
					        player.Out.SendPlayerCreate(this);
					        player.Out.SendLivingEquipmentUpdate(this);
				        }
			        }
		        }
	        }
        }

        public void UpdatePlayerStatus()
        {
	        Out.SendStatusUpdate();
        }
        
        public const string LAST_CHARGED_ITEM_USE_TICK = "LastChargedItemUsedTick";
        public const string ITEM_USE_DELAY = "ItemUseDelay";
        public const string NEXT_POTION_AVAIL_TIME = "LastPotionItemUsedTick";
        public const string NEXT_SPELL_AVAIL_TIME_BECAUSE_USE_POTION = "SpellAvailableTime";        
        
        protected bool m_enteredGame;
        protected bool m_targetInView;
        protected bool m_sitting = false;
        private bool m_stuckFlag = false;
        
        public bool EnteredGame
        {
	        get { return m_enteredGame; }
	        set { m_enteredGame = value; }
        }
        public override bool TargetInView
        {
	        get { return m_targetInView; }
	        set { m_targetInView = value; }
        }        
        public override bool IsSitting
        {
	        get { return m_sitting; }
	        set
	        {
		        m_sitting = value;
		        if (value)
		        {
			        if (IsCasting)
			        {
				        m_runningSpellHandler.CasterMoves();
			        }
		        }
	        }
        }
        public virtual bool Stuck
        {
	        get { return m_stuckFlag; }
	        set
	        {
		        if (value == m_stuckFlag) return;
		        m_stuckFlag = value;
	        }
        }

        #region Strafing
        public override bool IsMoving => base.IsMoving || IsStrafing;
        
		protected bool m_strafing;
		/// <summary>
		/// Gets/sets the current strafing mode
		/// </summary>
		public override bool IsStrafing
		{
			set
			{
				m_strafing = value;
				if (value)
				{
					OnPlayerMove();
				}
			}
			get { return m_strafing; }
		}

		public virtual void OnPlayerMove()
		{
			if (IsSitting)
			{
				Sit(false);
			}
			if (IsCasting)
			{
				m_runningSpellHandler.CasterMoves();
			}
			if (AttackState)
			{
				if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					string attackTypeMsg = (AttackWeapon.Object_Type == (int)eObjectType.Thrown ? "throw" : "shot");
					Out.SendMessage("You move and interrupt your " + attackTypeMsg + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					StopAttack();
				}
				else
				{
					AttackData ad = TempProperties.getProperty<object>(LAST_ATTACK_DATA, null) as AttackData;
					if (ad != null && ad.IsMeleeAttack && (ad.AttackResult == eAttackResult.TargetNotVisible || ad.AttackResult == eAttackResult.OutOfRange))
					{
						if (ad.Target != null && IsObjectInFront(ad.Target, 120) && this.IsWithinRadius(ad.Target, AttackRange) && m_attackAction != null)
						{
							m_attackAction.Start(1);
						}
					}
				}
			}
			GameEventManager.Notify(GamePlayerEvent.Moving, this);
		}
        #endregion

        #region mana / endu / con
		public virtual int CalculateMaxMana(int level, int manaStat)
		{
			int maxpower = 0;

			if (CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				maxpower = Math.Max(5, (level * 5) + (manaStat - 50));
			}
			else if (CharacterClass.ManaStat == eStat.UNDEFINED)
			{
				maxpower = 100;
			}
			if (maxpower < 0)
				maxpower = 0;

			return maxpower;
		}
		public override int Mana
		{
			get { return DBCharacter != null ? DBCharacter.Mana : base.Mana; }
			set
			{
				value = Math.Min(value, MaxMana);
				value = Math.Max(value, 0);
				//If it is already set, don't do anything
				if (Mana == value)
				{
					base.Mana = value; //needed to start regeneration
					return;
				}
				int oldPercent = ManaPercent;
				base.Mana = value;
				if (DBCharacter != null)
					DBCharacter.Mana = value;
				if (oldPercent != ManaPercent)
				{
					UpdatePlayerStatus();
				}
			}
		}
		public override int MaxMana
		{
			get { return GetModified(eProperty.MaxMana); }
		}
		public override int Endurance
		{
			get { return DBCharacter != null ? DBCharacter.Endurance : base.Endurance; }
			set
			{
				value = Math.Min(value, MaxEndurance);
				value = Math.Max(value, 0);
				//If it is already set, don't do anything
				if (Endurance == value)
				{
					base.Endurance = value; //needed to start regeneration
					return;
				}
				else if (IsAlive && value < MaxEndurance)
					StartEnduranceRegeneration();
				int oldPercent = EndurancePercent;
				base.Endurance = value;
				if (DBCharacter != null)
					DBCharacter.Endurance = value;
				if (oldPercent != EndurancePercent)
				{
					UpdatePlayerStatus();
				}
			}
		}

		/// <summary>
		/// Gets/sets the objects maximum endurance
		/// </summary>
		public override int MaxEndurance
		{
			get { return base.MaxEndurance; }
			set
			{
				//If it is already set, don't do anything
				if (MaxEndurance == value)
					return;
				base.MaxEndurance = value;
				DBMaxEndurance = value;
				UpdatePlayerStatus();
			}
		}
		
		public int DBMaxEndurance
		{
			get { return DBCharacter != null ? DBCharacter.MaxEndurance : 100; }
			set { if (DBCharacter != null) DBCharacter.MaxEndurance = value; }
		}
		
		public override int Concentration
		{
			get { return MaxConcentration - ConcentrationEffects.UsedConcentration; }
		}
		public override int MaxConcentration
		{
			get { return GetModified(eProperty.MaxConcentration); }
		}
        
		public override int Health
		{
			get { return DBCharacter != null ? DBCharacter.Health : base.Health; }
			set
			{
				value = value.Clamp(0, MaxHealth);
				//If it is already set, don't do anything
				if (Health == value)
				{
					base.Health = value; //needed to start regeneration
					return;
				}

				int oldPercent = HealthPercent;
				if (DBCharacter != null)
					DBCharacter.Health = value;
				base.Health = value;
				if (oldPercent != HealthPercent)
				{
					UpdatePlayerStatus();
				}
			}
		}

		public virtual int CalculateMaxHealth(int level, int constitution)
		{
			constitution -= 50;
			if (constitution < 0) constitution *= 2;

			int hp1 = CharacterClass.BaseHP * level;
			int hp2 = hp1 * constitution / 10000;
			int hp3 = 0;
			double hp4 = 20 + hp1 / 50 + hp2 + hp3;
			return Math.Max(1, (int)hp4);
		}
        #endregion
        
		#region Spells/Skills/Abilities/Effects
		/// <summary>
		/// Holds the player choosen list of Realm Abilities.
		/// </summary>
		protected readonly ReaderWriterList<RealmAbility> m_realmAbilities = new ReaderWriterList<RealmAbility>();
		
		/// <summary>
		/// Holds the player specializable skills and style lines
		/// (KeyName -> Specialization)
		/// </summary>
		protected readonly Dictionary<string, Specialization> m_specialization = new Dictionary<string, Specialization>();

		/// <summary>
		/// Holds the Spell lines the player can use
		/// </summary>
		protected readonly List<SpellLine> m_spellLines = new List<SpellLine>();

		/// <summary>
		/// Object to use when locking the SpellLines list
		/// </summary>
		protected readonly Object lockSpellLinesList = new Object();

		/// <summary>
		/// Holds all styles of the player
		/// </summary>
		protected readonly Dictionary<int, Style> m_styles = new Dictionary<int, Style>();

		/// <summary>
		/// Used to lock the style list
		/// </summary>
		protected readonly Object lockStyleList = new Object();

		/// <summary>
		/// Temporary Stats Boni
		/// </summary>
		protected readonly int[] m_statBonus = new int[8];

		/// <summary>
		/// Temporary Stats Boni in percent
		/// </summary>
		protected readonly int[] m_statBonusPercent = new int[8];

		/// <summary>
		/// Gets/Sets amount of full skill respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual int RespecAmountAllSkill
		{
			get { return DBCharacter != null ? DBCharacter.RespecAmountAllSkill : 0; }
			set { if (DBCharacter != null) DBCharacter.RespecAmountAllSkill = value; }
		}

		/// <summary>
		/// Gets/Sets amount of single-line respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual int RespecAmountSingleSkill
		{
			get { return DBCharacter != null ? DBCharacter.RespecAmountSingleSkill : 0; }
			set { if (DBCharacter != null) DBCharacter.RespecAmountSingleSkill = value; }
		}

		/// <summary>
		/// Gets/Sets amount of realm skill respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual int RespecAmountRealmSkill
		{
			get { return DBCharacter != null ? DBCharacter.RespecAmountRealmSkill : 0; }
			set { if (DBCharacter != null) DBCharacter.RespecAmountRealmSkill = value; }
		}

		/// <summary>
		/// Gets/Sets amount of DOL respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual int RespecAmountDOL
		{
			get { return DBCharacter != null ? DBCharacter.RespecAmountDOL : 0; }
			set { if (DBCharacter != null) DBCharacter.RespecAmountDOL = value; }
		}

		/// <summary>
		/// Gets/Sets level respec usage flag
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual bool IsLevelRespecUsed
		{
			get { return DBCharacter != null ? DBCharacter.IsLevelRespecUsed : true; }
			set { if (DBCharacter != null) DBCharacter.IsLevelRespecUsed = value; }
		}


		protected static readonly int[] m_numRespecsCanBuyOnLevel =
		{
			1,1,1,1,1, //1-5
			2,2,2,2,2,2,2, //6-12
			3,3,3,3, //13-16
			4,4,4,4,4,4, //17-22
			5,5,5,5,5, //23-27
			6,6,6,6,6,6, //28-33
			7,7,7,7,7, //34-38
			8,8,8,8,8,8, //39-44
			9,9,9,9,9, //45-49
			10 //50
		};


		/// <summary>
		/// Can this player buy a respec?
		/// </summary>
		public virtual bool CanBuyRespec
		{
			get
			{
				return (RespecBought < m_numRespecsCanBuyOnLevel[Level - 1]);
			}
		}

		/// <summary>
		/// Gets/Sets amount of bought respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual int RespecBought
		{
			get { return DBCharacter != null ? DBCharacter.RespecBought : 0; }
			set { if (DBCharacter != null) DBCharacter.RespecBought = value; }
		}


		protected static readonly int[] m_respecCost =
		{
			1,2,3, //13
			2,5,9, //14
			3,9,17, //15
			6,16,30, //16
			10,26,48,75, //17
			16,40,72,112, //18
			22,56,102,159, //19
			31,78,140,218, //20
			41,103,187,291, //21
			54,135,243,378, //22
			68,171,308,480,652, //23
			85,214,385,600,814, //24
			105,263,474,738,1001, //25
			128,320,576,896,1216, //26
			153,383,690,1074,1458, //27
			182,455,820,1275,1731,2278, //28
			214,535,964,1500,2036,2679, //29
			250,625,1125,1750,2375,3125, //30
			289,723,1302,2025,2749,3617, //31
			332,831,1497,2329,3161,4159, //32
			380,950,1710,2661,3612,4752, //33
			432,1080,1944,3024,4104,5400,6696, //34
			488,1220,2197,3417,4638,6103,7568, //35
			549,1373,2471,3844,5217,6865,8513, //36
			615,1537,2767,4305,5843,7688,9533, //37
			686,1715,3087,4802,6517,8575,10633, //38
			762,1905,3429,5335,7240,9526,11813,14099, //39
			843,2109,3796,5906,8015,10546,13078,15609, //40
			930,2327,4189,6516,8844,11637,14430,17222, //41
			1024,2560,4608,7168,9728,1280,15872,18944, //42
			1123,2807,5053,7861,10668,14037,17406,20776, //43
			1228,3070,5527,8597,11668,15353,19037,22722, //44
			1339,3349,6029,9378,12725,16748,20767,24787,28806, //45
			1458,3645,6561,10206,13851,18225,22599,26973,31347, //46
			1582,3957,7123,11080,15037,19786,24535,29283,34032, //47
			1714,4286,7716,12003,16290,21434,26578,31722,36867, //48
			1853,4634,8341,12976,17610,23171,28732,34293,39854, //49
			2000,5000,9000,14000,19000,25000,31000,37000,43000,50000 //50
		};


		/// <summary>
		/// How much does this player have to pay for a respec?
		/// </summary>
		public virtual long RespecCost
		{
			get
			{
				if (Level <= 12) //1-12
					return m_respecCost[0];

				if (CanBuyRespec)
				{
					int t = 0;
					for (int i = 13; i < Level; i++)
					{
						t += m_numRespecsCanBuyOnLevel[i - 1];
					}

					return m_respecCost[t + RespecBought];
				}

				return -1;
			}
		}

		/// <summary>
		/// give player a new Specialization or improve existing one
		/// </summary>
		/// <param name="skill"></param>
		public void AddSpecialization(Specialization skill)
		{
			AddSpecialization(skill, true);
		}
		

		/// <summary>
		/// give player a new Specialization or improve existing one
		/// </summary>
		/// <param name="skill"></param>
		protected virtual void AddSpecialization(Specialization skill, bool notify)
		{
			if (skill == null)
				return;

			lock (((ICollection)m_specialization).SyncRoot)
			{
				// search for existing key
				if (!m_specialization.ContainsKey(skill.KeyName))
				{
					// Adding
					m_specialization.Add(skill.KeyName, skill);

					if (skill.KeyName == Specs.Stealth)
						CanStealth = true;

					if (notify)
						Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.AddSpecialisation.YouLearn", skill.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					// Updating
					m_specialization[skill.KeyName].Level = skill.Level;
				}
			}
		}

		/// <summary>
		/// Removes the existing specialization from the player
		/// </summary>
		/// <param name="specKeyName">The spec keyname to remove</param>
		/// <returns>true if removed</returns>
		public virtual bool RemoveSpecialization(string specKeyName)
		{
			Specialization playerSpec = null;
			
			lock (((ICollection)m_specialization).SyncRoot)
			{
				if (!m_specialization.TryGetValue(specKeyName, out playerSpec))
						return false;
				
				m_specialization.Remove(specKeyName);
				
				if (specKeyName == Specs.Stealth)
					CanStealth = false;
			}
			
			Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RemoveSpecialization.YouLose", playerSpec.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return true;
		}

		/// <summary>
		/// Removes the existing spellline from the player, the line instance should be called with GamePlayer.GetSpellLine ONLY and NEVER SkillBase.GetSpellLine!!!!!
		/// </summary>
		/// <param name="line">The spell line to remove</param>
		/// <returns>true if removed</returns>
		protected virtual bool RemoveSpellLine(SpellLine line)
		{
			lock (lockSpellLinesList)
			{
				if (!m_spellLines.Contains(line))
				{
					return false;
				}

				m_spellLines.Remove(line);
			}

			Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RemoveSpellLine.YouLose", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			
			return true;
		}

		/// <summary>
		/// Removes the existing specialization from the player
		/// </summary>
		/// <param name="lineKeyName">The spell line keyname to remove</param>
		/// <returns>true if removed</returns>
		public virtual bool RemoveSpellLine(string lineKeyName)
		{
			SpellLine line = GetSpellLine(lineKeyName);
			if (line == null)
				return false;

			return RemoveSpellLine(line);
		}

		/// <summary>
		/// Reset this player to level 1, respec all skills, remove all spec points, and reset stats
		/// </summary>
		public virtual void Reset()
		{
			byte originalLevel = Level;
			Level = 1;
			RespecAllLines();

			if (Level < originalLevel && originalLevel > 5)
			{
				for (int i = 6; i <= originalLevel; i++)
				{
					if (CharacterClass.PrimaryStat != eStat.UNDEFINED)
					{
						ChangeBaseStat(CharacterClass.PrimaryStat, -1);
					}
					if (CharacterClass.SecondaryStat != eStat.UNDEFINED && ((i - 6) % 2 == 0))
					{
						ChangeBaseStat(CharacterClass.SecondaryStat, -1);
					}
					if (CharacterClass.TertiaryStat != eStat.UNDEFINED && ((i - 6) % 3 == 0))
					{
						ChangeBaseStat(CharacterClass.TertiaryStat, -1);
					}
				}
			}
		}

		public virtual bool RespecAll()
		{
			if(RespecAllLines())
			{
				// Wipe skills and styles.
				RespecAmountAllSkill--; // Decriment players respecs available.
				if (Level == 5)
					IsLevelRespecUsed = true;
				
				return true;
			}

			return false;
		}

		public virtual int GetAutoTrainPoints(Specialization spec, int Mode)
		{
			return 0;
		}
		
		public virtual bool RespecDOL()
		{
			if(RespecAllLines()) // Wipe skills and styles.
			{
				RespecAmountDOL--; // Decriment players respecs available.
				return true;
			}

			return false;
		}

		public virtual int RespecSingle(Specialization specLine)
		{
			int specPoints = RespecSingleLine(specLine); // Wipe skills and styles.

			RespecAmountSingleSkill--; // Decriment players respecs available.
			if (Level == 20 || Level == 40)
			{
				IsLevelRespecUsed = true;
			}
			return specPoints;
		}

		public virtual bool RespecRealm()
		{
			bool any = m_realmAbilities.Count > 0;
			
			foreach (Ability ab in m_realmAbilities)
				RemoveAbility(ab.KeyName);
			
			m_realmAbilities.Clear();
			RespecAmountRealmSkill--;
			return any;
		}

		protected virtual bool RespecAllLines()
		{
			bool ok = false;
			IList<Specialization> specList = GetSpecList().Where(e => e.Trainable).ToList();
			foreach (Specialization cspec in specList)
			{
				if (cspec.Level < 2)
					continue;
				RespecSingleLine(cspec);
				ok = true;
			}
			return ok;
		}

		/// <summary>
		/// Respec single line
		/// </summary>
		/// <param name="specLine">spec line being respec'd</param>
		/// <returns>Amount of points spent in that line</returns>
		protected virtual int RespecSingleLine(Specialization specLine)
		{
			int specPoints = (specLine.Level * (specLine.Level + 1) - 2) / 2;
			// Graveen - autotrain 1.87
			specPoints -= GetAutoTrainPoints(specLine, 0);

			//setting directly the autotrain points in the spec
			if (GetAutoTrainPoints(specLine, 4) == 1 && Level >= 8)
			{
				specLine.Level = (int)Math.Floor((double)Level / 4);
			}
			else specLine.Level = 1;
			
			return specPoints;
		}

		/// <summary>
		/// Send this players trainer window
		/// </summary>
		public virtual void SendTrainerWindow()
		{
			Out.SendTrainerWindow();
		}

		/// <summary>
		/// returns a list with all specializations
		/// in the order they were added
		/// </summary>
		/// <returns>list of Spec's</returns>
		public virtual IList<Specialization> GetSpecList()
		{
			List<Specialization> list;

			lock (((ICollection)m_specialization).SyncRoot)
			{
				// sort by Level and ID to simulate "addition" order... (try to sort your DB if you want to change this !)
				list = m_specialization.Select(item => item.Value).OrderBy(it => it.LevelRequired).ThenBy(it => it.ID).ToList();
			}
			
			return list;
		}

		/// <summary>
		/// returns a list with all non trainable skills without styles
		/// This is a copy of Ability until any unhandled Skill subclass needs to go in there...
		/// </summary>
		/// <returns>list of Skill's</returns>
		public virtual IList GetNonTrainableSkillList()
		{
			return GetAllAbilities();
		}

		/// <summary>
		/// Retrives a specific specialization by name
		/// </summary>
		/// <param name="name">the name of the specialization line</param>
		/// <param name="caseSensitive">false for case-insensitive compare</param>
		/// <returns>found specialization or null</returns>
		public virtual Specialization GetSpecializationByName(string name, bool caseSensitive = false)
		{
			Specialization spec = null;

			lock (((ICollection)m_specialization).SyncRoot)
			{
				
				if (caseSensitive && m_specialization.ContainsKey(name))
					spec = m_specialization[name];
				
				foreach (KeyValuePair<string, Specialization> entry in m_specialization)
				{
					if (entry.Key.ToLower().Equals(name.ToLower()))
					{
					    spec = entry.Value;
					    break;
					}
				}
			}

			return spec;
		}

		/// <summary>
		/// The best armor level this player can use.
		/// </summary>
		public virtual int BestArmorLevel
		{
			get
			{
				int bestLevel = -1;
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.AlbArmor));
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.HibArmor));
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.MidArmor));
				return bestLevel;
			}
		}

		#region Abilities

		/// <summary>
		/// Adds a new Ability to the player
		/// </summary>
		/// <param name="ability"></param>
		/// <param name="sendUpdates"></param>
		public override void AddAbility(Ability ability, bool sendUpdates)
		{
			if (ability == null)
				return;
			
			base.AddAbility(ability, sendUpdates);
		}

		/// <summary>
		/// Adds a Realm Ability to the player
		/// </summary>
		/// <param name="ability"></param>
		/// <param name="sendUpdates"></param>
		public virtual void AddRealmAbility(RealmAbility ability, bool sendUpdates)
		{
			if (ability == null)
				return;
			
			m_realmAbilities.FreezeWhile(list => {
			                             	int index = list.FindIndex(ab => ab.KeyName == ability.KeyName);
			                             	if (index > -1)
			                             	{
			                             		list[index].Level = ability.Level;
			                             	}
			                             	else
			                             	{
			                             		list.Add(ability);
			                             	}
			                             });
			
			RefreshSpecDependantSkills(true);
		}

		#endregion Abilities

		#region Send/Say/Yell/Whisper/Messages
		public virtual void MessageFromArea(GameObject source, string message, eChatType chatType, eChatLoc chatLocation)
		{
			Out.SendMessage(message, chatType, chatLocation);
		}
		public override void MessageFromControlled(string message, eChatType chatType)
		{
			MessageToSelf(message, chatType);
		}
		public override void MessageToSelf(string message, eChatType chatType)
		{
			Out.SendMessage(message, chatType, eChatLoc.CL_SystemWindow);
		}	
		public override bool Whisper(GameObject target, string str)
		{
			if (target == null)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network?.Account.Language, "GamePlayer.Whisper.SelectTarget"), eChatType.CT_System,
					eChatLoc.CL_ChatWindow);
				return false;
			}
			if (!base.Whisper(target, str))
				return false;
			if (target is GamePlayer)
				Out.SendMessage("You whisper, \"" + str + "\" to " + target.GetName(0, false), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}	
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str))
				return false;
			if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
			{
				Out.SendMessage(source.GetName(0, false) + " whispers to you, \"" + str + "\"", eChatType.CT_Say,
					eChatLoc.CL_ChatWindow);
			}
			else
			{
				Out.SendMessage(source.GetName(0, false) + " whispers something in a language you don't understand.",
					eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			}
			return true;
		}
		public override bool Yell(string str)
		{
			if (!base.Yell(str))
				return false;
			Out.SendMessage("You yell, \"" + str + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}
		public override bool YellReceive(GameLiving source, string str)
		{
			if (!base.YellReceive(source, str))
				return false;
			if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
			{
				Out.SendMessage(source.GetName(0, false) + " yells, \"" + str + "\"", eChatType.CT_Say,
					eChatLoc.CL_ChatWindow);
			}
			else
			{
				Out.SendMessage(source.GetName(0, false) + " yells something in a language you don't understand.",
					eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			}

			return true;
		}	
		public override bool Say(string str)
		{
			if (!base.Say(str))
				return false;
			Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Say.YouSay", str), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}
		public override bool SayReceive(GameLiving source, string str)
		{
			if (!base.SayReceive(source, str))
				return false;

			if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.SayReceive.Says", source.GetName(0, false), str),
					eChatType.CT_Say, eChatLoc.CL_ChatWindow);				
			}
			else
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.SayReceive.FalseLanguage", source.GetName(0, false)), eChatType.CT_Say, eChatLoc.CL_ChatWindow);	
			}
			
			return true;
		}		
		#endregion
		
		public virtual void RemoveAllAbilities()
		{
			lock (m_lockAbilities)
			{
				m_abilities.Clear();
			}
		}
		
		public virtual void RemoveAllSpecs()
		{
			lock (((ICollection)m_specialization).SyncRoot)
			{
				m_specialization.Clear();
			}
		}

		public virtual void RemoveAllSpellLines()
		{
			lock (lockSpellLinesList)
			{
				m_spellLines.Clear();
			}
		}

		public virtual void RemoveAllStyles()
		{
			lock (lockStyleList)
			{
				m_styles.Clear();
			}
		}

		public virtual void AddStyle(Style st, bool notify)
		{
			lock (lockStyleList)
			{
				if (m_styles.ContainsKey(st.ID))
				{
					m_styles[st.ID].Level = st.Level;
				}
				else
				{
					m_styles.Add(st.ID, st);
					
					// Verbose
					if (notify)
					{
						Style style = st;
						Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.YouLearn", style.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
	
						string message = null;
						
						if (Style.eOpening.Offensive == style.OpeningRequirementType)
						{
							switch (style.AttackResultRequirement)
							{
								case Style.eAttackResultRequirement.Style:
								case Style.eAttackResultRequirement.Hit: // TODO: make own message for hit after styles DB is updated
	
									Style reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, CharacterClass.ID);
									
									if (reqStyle == null)
										message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.AfterStyle", "(style " + style.OpeningRequirementValue + " not found)");
									
									else message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.AfterStyle", reqStyle.Name);
	
								break;
								case Style.eAttackResultRequirement.Miss: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.AfterMissed");
								break;
								case Style.eAttackResultRequirement.Parry: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.AfterParried");
								break;
								case Style.eAttackResultRequirement.Block: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.AfterBlocked");
								break;
								case Style.eAttackResultRequirement.Evade: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.AfterEvaded");
								break;
								case Style.eAttackResultRequirement.Fumble: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.AfterFumbles");
								break;
							}
						}
						else if (Style.eOpening.Defensive == style.OpeningRequirementType)
						{
							switch (style.AttackResultRequirement)
							{
								case Style.eAttackResultRequirement.Miss: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.TargetMisses");
								break;
								case Style.eAttackResultRequirement.Hit: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.TargetHits");
								break;
								case Style.eAttackResultRequirement.Parry: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.TargetParried");
								break;
								case Style.eAttackResultRequirement.Block: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.TargetBlocked");
								break;
								case Style.eAttackResultRequirement.Evade: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.TargetEvaded");
								break;
								case Style.eAttackResultRequirement.Fumble: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.TargetFumbles");
								break;
								case Style.eAttackResultRequirement.Style: message = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.RefreshSpec.TargetStyle");
								break;
							}
						}
	
						if (!Util.IsEmpty(message))
							Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// Retrieve this player Realm Abilities.
		/// </summary>
		/// <returns></returns>
		public virtual List<RealmAbility> GetRealmAbilities()
		{
			return m_realmAbilities.ToList();
		}
		
		/// <summary>
		/// Asks for existance of specific specialization
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public virtual bool HasSpecialization(string keyName)
		{
			bool hasit = false;
			
			lock (((ICollection)m_specialization).SyncRoot)
			{
				hasit = m_specialization.ContainsKey(keyName);
			}
			
			return hasit;
		}

		/// <summary>
		/// Checks whether Living has ability to use lefthanded weapons
		/// </summary>
		public override bool CanUseLefthandedWeapon
		{
			get
			{
				return CharacterClass.CanUseLefthandedWeapon;
			}
		}

		/// <summary>
		/// Calculates how many times left hand swings
		/// </summary>
		public override int CalculateLeftHandSwingCount()
		{
			if (CanUseLefthandedWeapon == false)
				return 0;

			if (GetBaseSpecLevel(Specs.Left_Axe) > 0)
				return 1; // always use left axe

			int specLevel = Math.Max(GetModifiedSpecLevel(Specs.Celtic_Dual), GetModifiedSpecLevel(Specs.Dual_Wield));
			specLevel = Math.Max(specLevel, GetModifiedSpecLevel(Specs.Fist_Wraps));
			if (specLevel > 0)
			{
				return RandomUtil.Chance(25 + (specLevel - 1) * 68 / 100) ? 1 : 0;
			}

			// HtH chance
			specLevel = GetModifiedSpecLevel(Specs.HandToHand);
			InventoryItem attackWeapon = AttackWeapon;
			InventoryItem leftWeapon = (Inventory == null) ? null : Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			if (specLevel > 0 && ActiveWeaponSlot == eActiveWeaponSlot.Standard
			    && attackWeapon != null && attackWeapon.Object_Type == (int)eObjectType.HandToHand &&
			    leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.HandToHand)
			{
				specLevel--;
				int randomChance = RandomUtil.Int(99);
				int hitChance = specLevel >> 1;
				if (randomChance < hitChance)
					return 1; // 1 hit = spec/2

				hitChance += specLevel >> 2;
				if (randomChance < hitChance)
					return 2; // 2 hits = spec/4

				hitChance += specLevel >> 4;
				if (randomChance < hitChance)
					return 3; // 3 hits = spec/16

				return 0;
			}

			return 0;
		}

		/// <summary>
		/// Returns a multiplier to adjust left hand damage
		/// </summary>
		/// <returns></returns>
		public override double CalculateLeftHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
		{
			double effectiveness = 1.0;

			if (CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.LeftAxe && mainWeapon != null &&
			    (mainWeapon.Item_Type == Slot.RIGHTHAND || mainWeapon.Item_Type == Slot.LEFTHAND))
			{
				int LASpec = GetModifiedSpecLevel(Specs.Left_Axe);
				if (LASpec > 0)
				{
					effectiveness = 0.625 + 0.0034 * LASpec;
				}
			}

			return effectiveness;
		}

		/// <summary>
		/// Returns a multiplier to adjust right hand damage
		/// </summary>
		/// <param name="leftWeapon"></param>
		/// <returns></returns>
		public override double CalculateMainHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
		{
			double effectiveness = 1.0;

			if (CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.LeftAxe && mainWeapon != null &&
			    (mainWeapon.Item_Type == Slot.RIGHTHAND || mainWeapon.Item_Type == Slot.LEFTHAND))
			{
				int LASpec = GetModifiedSpecLevel(Specs.Left_Axe);
				if (LASpec > 0)
				{
					effectiveness = 0.625 + 0.0034 * LASpec;
				}
			}

			return effectiveness;
		}


		/// <summary>
		/// returns the level of a specialization
		/// if 0 is returned, the spec is non existent on player
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public override int GetBaseSpecLevel(string keyName)
		{
			Specialization spec = null;
			int level = 0;
			
			lock (((ICollection)m_specialization).SyncRoot)
			{
				if (m_specialization.TryGetValue(keyName, out spec))
					level = m_specialization[keyName].Level;
			}
			
			return level;
		}

		/// <summary>
		/// returns the level of a specialization + bonuses from RR and Items
		/// if 0 is returned, the spec is non existent on the player
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public override int GetModifiedSpecLevel(string keyName)
		{
			if (keyName.StartsWith(GlobalSpellsLines.Champion_Lines_StartWith))
				return 50;

			Specialization spec = null;
			int level = 0;
			lock (((ICollection)m_specialization).SyncRoot)
			{
				if (!m_specialization.TryGetValue(keyName, out spec))
				{
					// if (keyName == GlobalSpellsLines.Combat_Styles_Effect)
					// {
					// 	if (CharacterClass.ID == (int)eCharacterClass.Reaver || CharacterClass.ID == (int)eCharacterClass.Heretic)
					// 		level = GetModifiedSpecLevel(Specs.Flexible);
					// 	if (CharacterClass.ID == (int)eCharacterClass.Valewalker)
					// 		level = GetModifiedSpecLevel(Specs.Scythe);
					// 	if (CharacterClass.ID == (int)eCharacterClass.Savage)
					// 		level = GetModifiedSpecLevel(Specs.Savagery);
					// }
	
					level = 0;
				}
			}
			
			if (spec != null)
			{
				level = spec.Level;
				// TODO: should be all in calculator later, right now
				// needs specKey -> eProperty conversion to find calculator and then
				// needs eProperty -> specKey conversion to find how much points player has spent
				eProperty skillProp = SkillBase.SpecToSkill(keyName);
				if (skillProp != eProperty.Undefined)
					level += GetModified(skillProp);
			}
				
			return level;
		}

		/// <summary>
		/// Adds a spell line to the player
		/// </summary>
		/// <param name="line"></param>
		public virtual void AddSpellLine(SpellLine line)
		{
			AddSpellLine(line, true);
		}
		
		/// <summary>
		/// Adds a spell line to the player
		/// </summary>
		/// <param name="line"></param>
		public virtual void AddSpellLine(SpellLine line, bool notify)
		{
			if (line == null)
				return;

			SpellLine oldline = GetSpellLine(line.KeyName);
			if (oldline == null)
			{
				lock (lockSpellLinesList)
				{
					m_spellLines.Add(line);
				}
				
				if (notify)
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.AddSpellLine.YouLearn", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				// message to player
				if (notify && oldline.Level < line.Level)
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.UpdateSpellLine.GainPower", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				oldline.Level = line.Level;
			}
		}

		/// <summary>
		/// return a list of spell lines in the order they were added
		/// this is a copy only.
		/// </summary>
		/// <returns></returns>
		public virtual List<SpellLine> GetSpellLines()
		{
			List<SpellLine> list = new List<SpellLine>();
			lock (lockSpellLinesList)
			{
				list = new List<SpellLine>(m_spellLines);
			}
			
			return list;
		}

		/// <summary>
		/// find a spell line on player and return them
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public virtual SpellLine GetSpellLine(string keyname)
		{
			lock (lockSpellLinesList)
			{
				foreach (SpellLine line in m_spellLines)
				{
					if (line.KeyName == keyname)
						return line;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets a list of available styles
		/// This creates a copy
		/// </summary>
		public virtual IList GetStyleList()
		{
			List<Style> list = new List<Style>();
			lock (lockStyleList)
			{
				list = m_styles.Values.OrderBy(x => x.SpecLevelRequirement).ThenBy(y => y.ID).ToList();
			}
			return list;
		}
		
		/// <summary>
		/// Skill cache, maintained for network order on "skill use" request...
		/// Second item is for "Parent" Skill if applicable
		/// </summary>
		protected ReaderWriterList<Tuple<Skill, Skill>> m_usableSkills = new ReaderWriterList<Tuple<Skill, Skill>>();
		
		/// <summary>
		/// List Cast cache, maintained for network order on "spell use" request...
		/// Second item is for "Parent" SpellLine if applicable
		/// </summary>
		protected ReaderWriterList<Tuple<SpellLine, List<Skill>>> m_usableListSpells = new ReaderWriterList<Tuple<SpellLine, List<Skill>>>();
		
		/// <summary>
		/// Get All Usable Spell for a list Caster.
		/// </summary>
		/// <param name="update"></param>
		/// <returns></returns>
		public virtual List<Tuple<SpellLine, List<Skill>>> GetAllUsableListSpells(bool update = false)
		{
			List<Tuple<SpellLine, List<Skill>>> results = new List<Tuple<SpellLine, List<Skill>>>();
			
			if (!update)
			{
				if (m_usableListSpells.Count > 0)
					results = new List<Tuple<SpellLine, List<Skill>>>(m_usableListSpells);
				
				// return results if cache is valid.
				if (results.Count > 0)
					return results;
				
			}

			// lock during all update, even if replace only take place at end...
			m_usableListSpells.FreezeWhile(innerList => {

				List<Tuple<SpellLine, List<Skill>>> finalbase = new List<Tuple<SpellLine, List<Skill>>>();
				List<Tuple<SpellLine, List<Skill>>> finalspec = new List<Tuple<SpellLine, List<Skill>>>();
							
				// Add Lists spells ordered.
				foreach (Specialization spec in GetSpecList().Where(item => !item.HybridSpellList))
				{
					var spells = spec.GetLinesSpellsForLiving(this);

					foreach (SpellLine sl in spec.GetSpellLinesForLiving(this))
					{
						List<Tuple<SpellLine, List<Skill>>> working;
						if (sl.IsBaseLine)
						{
							working = finalbase;
						}
						else
						{
							working = finalspec;
						}
						
						List<Skill> sps = new List<Skill>();
						SpellLine key = spells.Keys.FirstOrDefault(el => el.KeyName == sl.KeyName);
						
						if (key != null && spells.ContainsKey(key))
						{
							foreach (Skill sp in spells[key])
							{
								sps.Add(sp);
							}
						}
						
						working.Add(new Tuple<SpellLine, List<Skill>>(sl, sps));
					}
				}
				
				// Linq isn't used, we need to keep order ! (SelectMany, GroupBy, ToDictionary can't be used !)
				innerList.Clear();
				foreach (var tp in finalbase)
				{
					innerList.Add(tp);
					results.Add(tp);
				}
	
				foreach (var tp in finalspec)
				{
					innerList.Add(tp);
					results.Add(tp);
				}
			                               });
			
			return results;
		}
		
		/// <summary>
		/// Get All Player Usable Skill Ordered in Network Order (usefull to check for useskill)
		/// This doesn't get player's List Cast Specs...
		/// </summary>
		/// <param name="update"></param>
		/// <returns></returns>
		public virtual List<Tuple<Skill, Skill>> GetAllUsableSkills(bool update = false)
		{
			List<Tuple<Skill, Skill>> results = new List<Tuple<Skill, Skill>>();
			
			if (!update)
			{

				if (m_usableSkills.Count > 0)
					results = new List<Tuple<Skill, Skill>>(m_usableSkills);
				
				// return results if cache is valid.
				if (results.Count > 0)
					return results;
			}
			
			// need to lock for all update.
			m_usableSkills.FreezeWhile(innerList => {

				IList<Specialization> specs = GetSpecList();
				List<Tuple<Skill, Skill>> copylist = new List<Tuple<Skill, Skill>>(innerList);
								
				// Add Spec
				foreach (Specialization spec in specs.Where(item => item.Trainable))
				{
					int index = innerList.FindIndex(e => (e.Item1 is Specialization) && ((Specialization)e.Item1).KeyName == spec.KeyName);
					
					if (index < 0)
					{
						// Specs must be appended to spec list
						innerList.Insert(innerList.Count(e => e.Item1 is Specialization), new Tuple<Skill, Skill>(spec, spec));
					}
					else
					{
						copylist.Remove(innerList[index]);
						// Replace...
						innerList[index] = new Tuple<Skill, Skill>(spec, spec);
					}
				}
								
				// Add Abilities (Realm ability should be a custom spec)
				// Abilities order should be saved to db and loaded each time								
				foreach (Specialization spec in specs)
				{
					foreach (Ability abv in spec.GetAbilitiesForLiving(this))
					{
						// We need the Instantiated Ability Object for Displaying Correctly According to Player "Activation" Method (if Available)
						Ability ab = GetAbility(abv.KeyName);
						
						if (ab == null)
							ab = abv;
						
						int index = innerList.FindIndex(k => (k.Item1 is Ability) && ((Ability)k.Item1).KeyName == ab.KeyName);
						
						if (index < 0)
						{
							// add
							innerList.Add(new Tuple<Skill, Skill>(ab, spec));
						}
						else
						{
							copylist.Remove(innerList[index]);
							// replace
							innerList[index] = new Tuple<Skill, Skill>(ab, spec);
						}
					}
				}

				// Add Hybrid spell
				foreach (Specialization spec in specs.Where(item => item.HybridSpellList))
				{
					int index = -1;
					foreach(KeyValuePair<SpellLine, List<Skill>> sl in spec.GetLinesSpellsForLiving(this))
					{
						foreach (Spell sp in sl.Value.Where(it => (it is Spell) && !((Spell)it).NeedInstrument).Cast<Spell>())
						{
							if (index < innerList.Count)
								index = innerList.FindIndex(index + 1, e => ((e.Item2 is SpellLine) && ((SpellLine)e.Item2).Spec == sl.Key.Spec) && (e.Item1 is Spell) && !((Spell)e.Item1).NeedInstrument);
							
							if (index < 0 || index >= innerList.Count)
							{
								// add
								innerList.Add(new Tuple<Skill, Skill>(sp, sl.Key));
								// disable replace
								index = innerList.Count;
							}
							else
							{
								copylist.Remove(innerList[index]);
								// replace
								innerList[index] = new Tuple<Skill, Skill>(sp, sl.Key);
							}
						}
					}
				}
				
				// Add Songs
				int songIndex = -1;
				foreach (Specialization spec in specs.Where(item => item.HybridSpellList))
				{					
					foreach(KeyValuePair<SpellLine, List<Skill>> sl in spec.GetLinesSpellsForLiving(this))
					{
						foreach (Spell sp in sl.Value.Where(it => (it is Spell) && ((Spell)it).NeedInstrument).Cast<Spell>())
						{
							if (songIndex < innerList.Count)
								songIndex = innerList.FindIndex(songIndex + 1, e => (e.Item1 is Spell) && ((Spell)e.Item1).NeedInstrument);
							
							if (songIndex < 0 || songIndex >= innerList.Count)
							{
								// add
								innerList.Add(new Tuple<Skill, Skill>(sp, sl.Key));
								// disable replace
								songIndex = innerList.Count;
							}
							else
							{
								copylist.Remove(innerList[songIndex]);
								// replace
								innerList[songIndex] = new Tuple<Skill, Skill>(sp, sl.Key);
							}
						}
					}
				}
				
				// Add Styles
				foreach (Specialization spec in specs)
				{
					foreach(Style st in spec.GetStylesForLiving(this))
					{
						int index = innerList.FindIndex(e => (e.Item1 is Style) && e.Item1.ID == st.ID);
						if (index < 0)
						{
							// add
							innerList.Add(new Tuple<Skill, Skill>(st, spec));
						}
						else
						{
							copylist.Remove(innerList[index]);
							// replace
							innerList[index] = new Tuple<Skill, Skill>(st, spec);
						}
					}
				}

				// clean all not re-enabled skills
				foreach (Tuple<Skill, Skill> item in copylist)
				{
					innerList.Remove(item);
				}
				
				foreach (Tuple<Skill, Skill> el in innerList)
					results.Add(el);
			                           });
			
			return results;
		}
		
		/// <summary>
		/// updates the list of available skills (dependent on caracter specs)
		/// </summary>
		/// <param name="sendMessages">sends "you learn" messages if true</param>
		public virtual void RefreshSpecDependantSkills(bool sendMessages)
		{
			// refresh specs
			LoadClassSpecializations(sendMessages);
			
			// lock specialization while refreshing...
			lock (((ICollection)m_specialization).SyncRoot)
			{
				foreach (Specialization spec in m_specialization.Values)
				{
					// check for new Abilities
					foreach (Ability ab in spec.GetAbilitiesForLiving(this))
					{
						if (!HasAbility(ab.KeyName) || GetAbility(ab.KeyName).Level < ab.Level)
							AddAbility(ab, sendMessages);
					}
					
					// check for new Styles
					foreach (Style st in spec.GetStylesForLiving(this))
					{
						AddStyle(st, sendMessages);
					}
					
					// check for new SpellLine
					foreach (SpellLine sl in spec.GetSpellLinesForLiving(this))
					{
						AddSpellLine(sl, sendMessages);
					}
					
				}
			}
		}
		
		public void OnSkillTrained(Specialization skill)
		{
			Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.OnSkillTrained.YouSpend", skill.Level, skill.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			Message.SystemToOthers(this, LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.OnSkillTrained.TrainsInVarious", GetName(0, true)), eChatType.CT_System);
			RefreshSpecDependantSkills(true);

			//Out.SendUpdatePlayerSkills();
		}
		
		protected double m_playereffectiveness = 1.0;

		public override double Effectiveness
		{
			get { return m_playereffectiveness; }
			set { m_playereffectiveness = value; }
		}

		/// <summary>
		/// Creates new effects list for this living.
		/// </summary>
		/// <returns>New effects list instance</returns>
		protected override GameEffectList CreateEffectsList()
		{
			return new GameEffectPlayerList(this);
		}

        #endregion
        
        #region Attack
		public override int AttackSpeed(params InventoryItem[] weapons)
		{
			if (weapons == null || weapons.Length < 1)
				return 0;

			int count = 0;
			double speed = 0;
			bool bowWeapon = true;

			for (int i = 0; i < weapons.Length; i++)
			{
				if (weapons[i] != null)
				{
					speed += weapons[i].SPD_ABS;
					count++;

					switch (weapons[i].Object_Type)
					{
						case (int)eObjectType.Fired:
						case (int)eObjectType.Longbow:
						case (int)eObjectType.Crossbow:
						case (int)eObjectType.RecurvedBow:
						case (int)eObjectType.CompositeBow:
							break;
						default:
							bowWeapon = false;
							break;
					}
				}
			}

			if (count < 1)
				return 0;

			speed /= count;

			int qui = Math.Min(250, GetModified(eProperty.Quickness)); //250 soft cap on quickness

			if (bowWeapon)
			{
				speed *= (1.0 - (qui - 60) * 0.002);
			} else
			{
				speed *= (1.0 - (qui - 60) * 0.002) * 0.01 * GetModified(eProperty.MeleeSpeed);
			}
			
			if (speed < 15)
			{
				speed = 15;
			}
			return (int)(speed * 100);
		}
		
		public override double AttackDamage(InventoryItem weapon)
		{
			if (weapon == null)
				return 0;

			double effectiveness = 1.00;
			double damage = WeaponDamage(weapon) * weapon.SPD_ABS * 0.1;

			if (weapon.Hand == 1) // two-hand
			{
				// twohanded used weapons get 2H-Bonus = 10% + (Skill / 2)%
				int spec = WeaponSpecLevel(weapon) - 1;
				damage *= 1.1 + spec * 0.005;
			}

			if (weapon.Item_Type == Slot.RANGED)
			{
				//Ranged damage buff,debuff,Relic,RA
				effectiveness += GetModified(eProperty.RangedDamage) * 0.01;
			}
			else if (weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type == Slot.TWOHAND)
			{
				//Melee damage buff,debuff,Relic,RA
				effectiveness += GetModified(eProperty.MeleeDamage) * 0.01;
			}
			damage *= effectiveness;
			return damage;
		}
		
		public override void Die(GameObject killer)
		{
			// ambiant talk
			if (killer is GameNPC)
				(killer as GameNPC).FireAmbientSentence(GameNPC.eAmbientTrigger.killing, this);

			bool realmDeath = killer != null && killer.Realm != eRealm.None;

			TargetObject = null;

			string playerMessage;
			string publicMessage;
			ushort messageDistance = WorldManager.DEATH_MESSAGE_DISTANCE;

			string location = "";
			if (CurrentAreas.Count > 0 && (CurrentAreas[0] is Area.BindArea) == false)
				location = (CurrentAreas[0] as AbstractArea).Description;
			else
				location = CurrentZone.Description;

			if (killer == null)
			{
				if (realmDeath)
				{
					playerMessage = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.KilledLocation", GetName(0, true), location);
                    publicMessage = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.KilledLocation", GetName(0, true), location);
				}
				else
				{
					playerMessage = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.Killed", GetName(0, true));
                    publicMessage = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.Killed", GetName(0, true));
				}
			}
			else
			{
				messageDistance = 0;
				if (realmDeath)
				{
					playerMessage = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.KilledByLocation", GetName(0, true), killer.GetName(1, false), location);
                    publicMessage = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.KilledByLocation", GetName(0, true), killer.GetName(1, false), location);
				}
				else
				{
					playerMessage = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.KilledBy", GetName(0, true), killer.GetName(1, false));
                    publicMessage = LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.KilledBy", GetName(0, true), killer.GetName(1, false));
				}
			}

			eChatType messageType = eChatType.CT_PlayerDied;
			
			if (killer != null)
			{
				switch ((eRealm)killer.Realm)
				{
						case eRealm.Albion: messageType = eChatType.CT_KilledByEnemy; break;
						case eRealm.Midgard: messageType = eChatType.CT_KilledByEnemy; break;
						case eRealm.Hibernia: messageType = eChatType.CT_KilledByEnemy; break;
						default: messageType = eChatType.CT_PlayerDied; break; // killed by mob
				}
			}

			if (killer is GamePlayer && killer != this)
			{
				((GamePlayer)killer).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)killer).Network.Account.Language, "GamePlayer.Die.YouKilled", GetName(0, false)), eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
			}

			ArrayList players = new ArrayList();
			if (messageDistance == 0)
			{
				foreach (GameClient client in WorldManager.GetClientsOfRegion(CurrentRegionID))
				{
					players.Add(client.Player);
				}
			}
			else
			{
				foreach (GamePlayer player in GetPlayersInRadius(messageDistance))
				{
					if (player == null) continue;
					players.Add(player);
				}
			}

			foreach (GamePlayer player in players)
			{
				// on normal server type send messages only to the killer and dead players realm
				// check for gameplayer is needed because killers realm don't see deaths by guards
				if ((player != killer) && (
						(killer != null && killer is GamePlayer && GameServer.ServerRules.IsSameRealm((GamePlayer)killer, player, true))
						|| (GameServer.ServerRules.IsSameRealm(this, player, true))))
					if (player == this)
						player.Out.SendMessage(playerMessage, messageType, eChatLoc.CL_SystemWindow);
				else player.Out.SendMessage(publicMessage, messageType, eChatLoc.CL_SystemWindow);
			}

			//Dead ppl. don't sit ...
			if (IsSitting)
			{
				IsSitting = false;
				UpdatePlayerStatus();
			}

			// then buffs drop messages
			base.Die(killer);

			lock (m_LockObject)
			{
				if (m_releaseTimer != null)
				{
					m_releaseTimer.Stop();
					m_releaseTimer = null;
				}

				if (m_quitTimer != null)
				{
					m_quitTimer.Stop();
					m_quitTimer = null;
				}
				m_automaticRelease = m_releaseType == eReleaseType.Duel;
				m_releasePhase = 0;
				m_deathTick = Environment.TickCount; // we use realtime, because timer window is realtime

				Out.SendTimerWindow(LanguageMgr.GetTranslation(Network.Account.Language, "System.ReleaseTimer"), (m_automaticRelease ? RELEASE_MINIMUM_WAIT : RELEASE_TIME));
				m_releaseTimer = new RegionTimer(this);
				m_releaseTimer.Callback = new RegionTimerCallback(ReleaseTimerCallback);
				m_releaseTimer.Start(1000);

				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.ReleaseToReturn"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);

				// clear target object so no more actions can used on this target, spells, styles, attacks...
				TargetObject = null;

				foreach (GamePlayer player in GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
				{
					if (player == null) continue;
					player.Out.SendPlayerDied(this, killer);
				}

				// first penalty is 5% of expforlevel, second penalty comes from release
				int xpLossPercent;
				if (Level < 40)
				{
					xpLossPercent = MaxLevel - Level;
				}
				else
				{
					xpLossPercent = MaxLevel - 40;
				}

				if (realmDeath) //Live PvP servers have 3 con loss on pvp death, can be turned off in server properties -Unty
				{
					int conpenalty = 0;
					switch (GameServer.Instance.Configuration.ServerType)
					{
						case eGameServerType.GST_Normal:
								Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.DeadRVR"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
								xpLossPercent = 0;
								m_deathtype = eDeathType.RvR;
								break;
								
						case eGameServerType.GST_PvP:
								Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.DeadRVR"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
								xpLossPercent = 0;
								m_deathtype = eDeathType.PvP;
								if (ServerProperties.Properties.PVP_DEATH_CON_LOSS)
								{
									conpenalty = 3;
									TempProperties.setProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, conpenalty);
								}
								break;
				 	}
					 
				}
				else
				{
                    if (Level >= ServerProperties.Properties.PVE_EXP_LOSS_LEVEL)
                    {
                        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.LoseExperience"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                        // if this is the first death in level, you lose only half the penalty
                        switch (DeathCount)
                        {
                            case 0:
                                Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.DeathN1"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                                xpLossPercent /= 3;
                                break;
                            case 1:
                                Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Die.DeathN2"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
                                xpLossPercent = xpLossPercent * 2 / 3;
                                break;
                        }

                        DeathCount++;
                        m_deathtype = eDeathType.PvE;
                        long xpLoss = (ExperienceForNextLevel - ExperienceForCurrentLevel) * xpLossPercent / 1000;
                        GainExperience(eXPSource.Other, -xpLoss, 0, 0, 0, false, true);
                        TempProperties.setProperty(DEATH_EXP_LOSS_PROPERTY, xpLoss);
                    }

                    if (Level >= ServerProperties.Properties.PVE_CON_LOSS_LEVEL)
                    {
                        int conLoss = DeathCount;
                        if (conLoss > 3)
                            conLoss = 3;
                        else if (conLoss < 1)
                            conLoss = 1;
                        TempProperties.setProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, conLoss);
                    }
				}
				GameEventMgr.AddHandler(this, GamePlayerEvent.Revive, new DOLEventHandler(OnRevive));
			}

			if (this.ControlledBrain != null)
				CommandNpcRelease();
			
			Message.SystemToOthers2(this, eChatType.CT_PlayerDied, "GamePlayer.Die.CorpseLies", GetName(0, true), GetPronoun(this.Client, 1, true));
			
			GameServer.ServerRules.OnPlayerKilled(this, killer);
			DeathTime = PlayedTime;
		}

		public override void EnemyKilled(GameLiving enemy)
		{
			if (ControlledBrain != null && ControlledBrain.Body.Attackers.Contains(enemy))
				ControlledBrain.Body.RemoveAttacker(enemy);

			base.EnemyKilled(enemy);
		}

		public override bool InCombat
		{
			get
			{
				IControlledBrain npc = ControlledBrain;
				if (npc != null && npc.Body.InCombat)
					return true;
				return base.InCombat;
			}
		}

		public override int GetDamageResist(eProperty property)
		{
			int res = 0;
			int classResist = 0;
			int secondResist = 0;

			switch ((eResist)property)
			{
				case eResist.Body:
				case eResist.Cold:
				case eResist.Energy:
				case eResist.Heat:
				case eResist.Matter:
				case eResist.Spirit:
					res += GetBuffBonus(eBuffBonusType.BaseBuff)[(int)eProperty.MagicAbsorption];
					break;
				default:
					break;
			}
			return (int)((res + classResist) - 0.01 * secondResist * (res + classResist) + secondResist);
		}		
        #endregion

        #region Damage
        public virtual eArmorSlot CalculateArmorHitLocation(AttackData ad)
        {
	        if (ad.Style != null)
	        {
		        if (ad.Style.ArmorHitLocation != eArmorSlot.NOTSET)
			        return ad.Style.ArmorHitLocation;
	        }
	        int chancehit = RandomUtil.Int(1, 100);
	        if (chancehit <= 40)
	        {
		        return eArmorSlot.TORSO;
	        }
	        else if (chancehit <= 65)
	        {
		        return eArmorSlot.LEGS;
	        }
	        else if (chancehit <= 80)
	        {
		        return eArmorSlot.ARMS;
	        }
	        else if (chancehit <= 90)
	        {
		        return eArmorSlot.HEAD;
	        }
	        else if (chancehit <= 95)
	        {
		        return eArmorSlot.HAND;
	        }
	        else
	        {
		        return eArmorSlot.FEET;
	        }
        }
        #endregion
        
        #region Detect
		public virtual bool CanDetect(GamePlayer enemy)
		{
			if (enemy.CurrentRegionID != CurrentRegionID)
				return false;
			if (!IsAlive)
				return false;
			if (this.Network.Account.PrivLevel > 1)
				return true;
			if (enemy.Network.Account.PrivLevel > 1)
				return false;

			int EnemyStealthLevel = enemy.GetModifiedSpecLevel(Specs.Stealth);
			if (EnemyStealthLevel > 50)
			{
				EnemyStealthLevel = 50;
			}
			int levelDiff = this.Level - EnemyStealthLevel;
			if (levelDiff < 0) levelDiff = 0;

			int range;
			bool enemyHasCamouflage = enemy.EffectList.GetOfType<CamouflageEffect>() != null;
			if (HasAbility(Abilities.DetectHidden) && !enemy.HasAbility(Abilities.DetectHidden) && !enemyHasCamouflage)
			{
				range = levelDiff * 50 + 250; // Detect Hidden advantage
			}
			else
			{
				range = levelDiff * 20 + 125; // Normal detection range
			}

			range += this.GetBuffBonus(eBuffBonusType.BaseBuff)[(int)eProperty.Skill_Stealth];

			if (range > 1900)
				range = 1900;
			
			return this.IsWithinRadius( enemy, range );
		}
        

        #endregion
        
        #region Guild
        private Guild.Guild m_guild;
        private DBRank m_guildRank;
        
        public Guild.Guild Guild
        {
	        get { return m_guild; }
	        set
	        {
		        if (value == null)
		        {
			        m_guild.RemoveOnlineMember(this);
		        }

		        m_guild = value;
		        if (ObjectState == eObjectState.Active)
		        {
			        Out.SendUpdatePlayer();
			        foreach (GamePlayer player in GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
			        {
				        if (player == null) continue;
				        if (player != this)
				        {
					        player.Out.SendObjectRemove(this);
					        player.Out.SendPlayerCreate(this);
					        player.Out.SendLivingEquipmentUpdate(this);
				        }
			        }
		        }
	        }
        }
        
        public DBRank GuildRank
        {
	        get { return m_guildRank; }
	        set
	        {
		        m_guildRank = value;
		        if (value != null && Network?.Account != null)
		        {
			        Network.Account.GuildRank = value.RankLevel;
		        }
	        }
        }

        /// <summary>
        /// Gets or sets the database guildid of this player
        /// (delegate to DBCharacter)
        /// </summary>
        public string GuildID
        {
	        get { return Network.Account.GuildID; }
	        set
	        {
		        Network.Account.GuildID = value;
	        }
        }
        #endregion

        #region Sprint
        protected SprintEffect m_sprintEffect = null;

        public bool IsSprinting
        {
	        get { return m_sprintEffect != null; }
        }

        public virtual bool Sprint(bool state)
        {
	        if (state == IsSprinting)
		        return state;

	        if (state)
	        {
		        // can't start sprinting with 10 endurance on 1.68 server
		        if (Endurance <= 10)
		        {
			        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.TooFatigued"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			        return false;
		        }
		        if (IsStealthed)
		        {
			        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.CantSprintHidden"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			        return false;
		        }
		        if (!IsAlive)
		        {
			        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.CantSprintDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			        return false;
		        }

		        m_sprintEffect = new SprintEffect();
		        m_sprintEffect.Start(this);
		        Out.SendUpdateMaxSpeed();
		        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.PrepareSprint"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		        return true;
	        }
	        else
	        {
		        m_sprintEffect.Stop();
		        m_sprintEffect = null;
		        Out.SendUpdateMaxSpeed();
		        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.NoLongerReady"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		        return false;
	        }
        }
        #endregion
        
		#region Player Quitting
		/// <summary>
		/// quit timer
		/// </summary>
		protected RegionTimer m_quitTimer;

		/// <summary>
		/// Timer callback for quit
		/// </summary>
		/// <param name="callingTimer">the calling timer</param>
		/// <returns>the new intervall</returns>
		protected virtual int QuitTimerCallback(RegionTimer callingTimer)
		{
			if (!IsAlive || ObjectState != eObjectState.Active)
			{
				m_quitTimer = null;
				return 0;
			}

			bool bInstaQuit = false;

			if (Network.Account.PrivLevel > 1) // GMs can always insta quit
				bInstaQuit = true;
			else if (ServerProperties.Properties.DISABLE_QUIT_TIMER && Network.Player.InCombat == false)  // Players can only insta quit if they aren't in combat
				bInstaQuit = true;

			if (bInstaQuit == false)
			{
				long lastCombatAction = LastAttackedByEnemyTick;
				if (lastCombatAction < LastAttackedTick)
				{
					lastCombatAction = LastAttackedTick;
				}
				long secondsleft = 60 - (CurrentRegion.Time - lastCombatAction + 500) / 1000; // 500 is for rounding
				if (secondsleft > 0)
				{
					if (secondsleft == 15 || secondsleft == 10 || secondsleft == 5)
					{
						Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.YouWillQuit1", secondsleft), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
					return 1000;
				}
			}

			Out.SendPlayerQuit(false);
			Quit(true);
			SaveIntoDatabase();
			m_quitTimer = null;
			return 0;
		}

		/// <summary>
		/// Gets the amount of time the player must wait before quit, in seconds
		/// </summary>
		public virtual int QuitTime
		{
			get
			{
				if (m_quitTimer == null)
				{
					// dirty trick ;-) (20sec min quit time)
					if (CurrentRegion.Time - LastAttackedTick > 40000)
						LastAttackedTick = CurrentRegion.Time - 40000;
				}
				long lastCombatAction = LastAttackedTick;
				if (lastCombatAction < LastAttackedByEnemyTick)
				{
					lastCombatAction = LastAttackedByEnemyTick;
				}
				return (int)(60 - (CurrentRegion.Time - lastCombatAction + 500) / 1000); // 500 is for rounding
			}
			set
			{ }
		}
		
		#endregion 
		
        public Position BindPosition
        {
            get
            {
                if(DBCharacter == null) return Position.Zero;
                return DBCharacter.GetBindPosition();
            }
            set
            {
                if (DBCharacter == null) return;

                DBCharacter.BindRegion = value.RegionID;
                DBCharacter.BindXpos = value.X;
                DBCharacter.BindYpos = value.Y;
                DBCharacter.BindZpos = value.Z;
                DBCharacter.BindHeading = value.Orientation.InHeading;
            }
        }
        public virtual bool MoveToBind()
        {
            Region rgn = WorldManager.GetRegion(BindPosition.RegionID);
            if (rgn == null || rgn.GetZone(BindPosition.Coordinate) == null)
            {
                Network?.Out.SendPlayerQuit(true);
                SaveIntoDatabase();
                Quit(true);

                //if (ServerProperties.Properties.BAN_HACKERS)
                {
                    DBBannedAccount b = new DBBannedAccount();
                    b.Author = "SERVER";

                    b.Ip = Network?.TcpEndpointAddress ?? "";
                    b.Account = AccountName;
                    b.DateBan = DateTime.Now;
                    b.Type = "B";
                    b.Reason = "X/Y/RegionID : " + Position.X + "/" + Position.Y + "/" + Position.RegionID;
                    GameServer.Database.AddObject(b);
                    GameServer.Database.SaveObject(b);
                    
                    if (Network != null && Network.IsConnected() == true)
                    {
	                    string message = "Unknown bind point, your account is banned, contact a GM.";
	                    Network.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
	                    Network.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
                    }
                }
                return false;
            }
            return MoveTo(BindPosition);
        }
		public virtual bool Quit(bool forced)
		{
			if (!forced)
			{
				if (!IsAlive)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.CantQuitDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (IsMoving)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.CantQuitStanding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				if (CurrentRegion.IsInstance)
				{
                    Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.CantQuitInInstance"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				

				if (!IsSitting)
				{
					Sit(true);
				}
				int secondsleft = QuitTime;

				if (m_quitTimer == null)
				{
					m_quitTimer = new RegionTimer(this);
					m_quitTimer.Callback = new RegionTimerCallback(QuitTimerCallback);
					m_quitTimer.Start(1);
				}

				if (secondsleft > 20)
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.RecentlyInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.YouWillQuit2", secondsleft), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				//Notify our event handlers (if any)
				Notify(GamePlayerEvent.Quit, this);

				//Cleanup stuff
				Delete();
			}
			return true;
		}
		public virtual void Sit(bool sit)
		{
			Sprint(false);

			if (IsSitting == sit)
			{
				if (sit)
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.AlreadySitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (!sit)
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.NotSitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (!IsAlive)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.CantSitDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsStunned)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.CantSitStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsMezzed)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.CantSitMezzed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (sit && CurrentSpeed > 0)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.MustStandingStill"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (sit)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.YouSitDown"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.YouStandUp"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			
			if (sit && AttackState)
			{
				StopAttack();
			}

			if (!sit)
			{
				if (m_quitTimer != null)
				{
					m_quitTimer.Stop();
					m_quitTimer = null;
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sit.NoLongerWaitingQuit"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}

			if (sit && !IsSitting)
			{
				Out.SendStatusUpdate(true);
			}

			IsSitting = sit;
			UpdatePlayerStatus();
		}
        protected virtual void CleanupOnDisconnect()
        {
	        StopAttack();
	        Stealth(false);
	        try
	        {
		        EffectList.SaveAllEffects();
		        CancelAllConcentrationEffects();
		        EffectList.CancelAll();
	        }
	        catch (Exception e)
	        {
		        log.ErrorFormat("Cannot cancel all effects - {0}", e);
	        }	        
        }

        public override void Delete()
        {
	        //Todo. 데이터 제거
        }

        #region Invulnerability
		public delegate void InvulnerabilityExpiredCallback(GamePlayer player);
		protected InvulnerabilityTimer m_invulnerabilityTimer;
		protected long m_invulnerabilityTick;
		public virtual bool StartInvulnerabilityTimer(int duration, InvulnerabilityExpiredCallback callback)
		{
			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvE)
				return false;

			if (duration < 1)
            {
	            return false;
            }

			long newTick = CurrentRegion.Time + duration;
			if (newTick < m_invulnerabilityTick)
				return false;

			m_invulnerabilityTick = newTick;
			if (m_invulnerabilityTimer != null)
				m_invulnerabilityTimer.Stop();

			if (callback != null)
			{
				m_invulnerabilityTimer = new InvulnerabilityTimer(this, callback);
				m_invulnerabilityTimer.Start(duration);
			}
			else
			{
				m_invulnerabilityTimer = null;
			}

			return true;
		}

		public virtual bool IsInvulnerableToAttack
		{
			get { return m_invulnerabilityTick > CurrentRegion.Time; }
		}

		protected class InvulnerabilityTimer : RegionAction
		{
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
			private readonly InvulnerabilityExpiredCallback m_callback;

			public InvulnerabilityTimer(GamePlayer actionSource, InvulnerabilityExpiredCallback callback)
				: base(actionSource)
			{
				if (callback == null)
					throw new ArgumentNullException("callback");
				m_callback = callback;
			}

			protected override void OnTick()
			{
				try
				{
					m_callback((GamePlayer)m_actionSource);
				}
				catch (Exception e)
				{
					log.Error("InvulnerabilityTimer callback", e);
				}
			}
		}
		#endregion

		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			if (!(obj is DOLCharacters))
			{
				return;
			}

			mdbCharacter = (DOLCharacters)obj;
			
			Wallet.InitializeFromDatabase();
			
			Model = (ushort)DBCharacter.CurrentModel;
			
			
			// 스탯 설정
			m_charStat[eStat.STR - eStat._First] = (short)DBCharacter.Strength;
			m_charStat[eStat.DEX - eStat._First] = (short)DBCharacter.Dexterity;
			m_charStat[eStat.CON - eStat._First] = (short)DBCharacter.Constitution;
			m_charStat[eStat.QUI - eStat._First] = (short)DBCharacter.Quickness;
			m_charStat[eStat.INT - eStat._First] = (short)DBCharacter.Intelligence;
			m_charStat[eStat.PIE - eStat._First] = (short)DBCharacter.Piety;
			m_charStat[eStat.EMP - eStat._First] = (short)DBCharacter.Empathy;
			m_charStat[eStat.CHR - eStat._First] = (short)DBCharacter.Charisma;
			
			SetCharacterClass(CharacterClass.GetClass(DBCharacter.Class));

			CurrentSpeed = 0;
			if (MaxSpeedBase == 0)
				MaxSpeedBase = PLAYER_BASE_SPEED;

			m_inventory.LoadFromDatabase(InternalID);
			
			SwitchWeapon((eActiveWeaponSlot)(DBCharacter.ActiveWeaponSlot & 0x0F));
			
			if (DBCharacter.PlayedTime < 1)
			{
				Health = MaxHealth;
				Mana = MaxMana;
				Endurance = MaxEndurance;
			}
			else
			{
				Health = DBCharacter.Health;
				Mana = DBCharacter.Mana;
				Endurance = DBCharacter.Endurance;
			}

			if (Health <= 0)
			{
				Health = 1;
			}
			
			LoadSkillsFromCharacter();
			
			DBCharacter.LastPlayed = DateTime.Now;
		}

		public long PlayedTime
		{
			get
			{
				DateTime rightNow = DateTime.Now;
				DateTime oldLast = LastPlayed;
				TimeSpan playaPlayed = rightNow.Subtract(oldLast);
				TimeSpan newPlayed = playaPlayed + TimeSpan.FromSeconds(DBCharacter.PlayedTime);
				return (long)newPlayed.TotalSeconds;
			}
		}

		/// <summary>
		/// Saves the player's skills
		/// </summary>
		protected virtual void SaveSkillsToCharacter()
		{
			StringBuilder ab = new StringBuilder();
			StringBuilder sp = new StringBuilder();
			
			// Build Serialized Spec list
			List<Specialization> specs = null;
			lock (((ICollection)m_specialization).SyncRoot)
			{
				specs = m_specialization.Values.Where(s => s.AllowSave).ToList();
				foreach (Specialization spec in specs)
				{
					if (sp.Length > 0)
					{
						sp.Append(";");
					}
					sp.AppendFormat("{0}|{1}", spec.KeyName, spec.GetSpecLevelForLiving(this));
				}
			}
			
			// Build Serialized Ability List to save Order
			foreach (Ability ability in m_usableSkills.Where(e => e.Item1 is Ability).Select(e => e.Item1).Cast<Ability>())
			{					
				if (ability != null)
				{
					if (ab.Length > 0)
					{
						ab.Append(";");
					}
					ab.AppendFormat("{0}|{1}", ability.KeyName, ability.Level);
				}
			}

			// Build Serialized disabled Spell/Ability
			StringBuilder disabledSpells = new StringBuilder();
			StringBuilder disabledAbilities = new StringBuilder();
			
			ICollection<Skill> disabledSkills = GetAllDisabledSkills();
			
			foreach (Skill skill in disabledSkills)
			{
				int duration = GetSkillDisabledDuration(skill);
				
				if (duration <= 0)
					continue;
				
				if (skill is Spell)
				{
					Spell spl = (Spell)skill;
					
					if (disabledSpells.Length > 0)
						disabledSpells.Append(";");
					
					disabledSpells.AppendFormat("{0}|{1}", spl.ID, duration);
				}
				else if (skill is Ability)
				{
					Ability ability = (Ability)skill;
					
					if (disabledAbilities.Length > 0)
						disabledAbilities.Append(";");
					
					disabledAbilities.AppendFormat("{0}|{1}", ability.KeyName, duration);
				}
				else
				{
					if (log.IsWarnEnabled)
						log.WarnFormat("{0}: Can't save disabled skill {1}", Name, skill.GetType().ToString());
				}
			}
			
			StringBuilder sra = new StringBuilder();
			
			if (DBCharacter != null)
			{
				DBCharacter.SerializedAbilities = ab.ToString();
				DBCharacter.SerializedSpecs = sp.ToString();
				DBCharacter.SerializedRealmAbilities = sra.ToString();
				DBCharacter.DisabledSpells = disabledSpells.ToString();
				DBCharacter.DisabledAbilities = disabledAbilities.ToString();
			}
		}
		
		protected virtual void LoadSkillsFromCharacter()
		{
			DOLCharacters character = DBCharacter; // if its derived and filled with some code
			if (character == null) return; // no character => exit

			#region load class spec
			
			// first load spec's career
			LoadClassSpecializations(false);
			
			//Load Remaining spec and levels from Database (custom spec can still be added here...)
			string tmpStr = character.SerializedSpecs;
			if (tmpStr != null && tmpStr.Length > 0)
			{
				foreach (string spec in Util.SplitCSV(tmpStr))
				{
					string[] values = spec.Split('|');
					if (values.Length >= 2)
					{
						Specialization tempSpec = SkillBase.GetSpecialization(values[0], false);

						if (tempSpec != null)
						{
							if (tempSpec.AllowSave)
							{
								int level;
								if (int.TryParse(values[1], out level))
								{
									if (HasSpecialization(tempSpec.KeyName))
									{
										GetSpecializationByName(tempSpec.KeyName).Level = level;
									}
									else
									{
										tempSpec.Level = level;
										AddSpecialization(tempSpec, false);
									}
								}
								else if (log.IsErrorEnabled)
								{
									log.ErrorFormat("{0} : error in loading specs => '{1}'", Name, tmpStr);
								}
							}
						}
						else if (log.IsErrorEnabled)
						{
							log.ErrorFormat("{0}: can't find spec '{1}'", Name, values[0]);
						}
					}
				}
			}
			
			// Add Serialized Abilities to keep Database Order
			// Custom Ability will be disabled as soon as they are not in any specs...
			tmpStr = character.SerializedAbilities;
			if (tmpStr != null && tmpStr.Length > 0 && m_usableSkills.Count == 0)
			{
				foreach (string abilities in Util.SplitCSV(tmpStr))
				{
					string[] values = abilities.Split('|');
					if (values.Length >= 2)
					{
						int level;
						if (int.TryParse(values[1], out level))
						{
							Ability ability = SkillBase.GetAbility(values[0], level);
							if (ability != null)
							{
								// this is for display order only
								m_usableSkills.Add(new Tuple<Skill, Skill>(ability, ability));
							}
						}
					}
				}
			}
			
			// Retrieve Realm Abilities From Database to be handled by Career Spec
			tmpStr = character.SerializedRealmAbilities;
			if (tmpStr != null && tmpStr.Length > 0)
			{
				foreach (string abilities in Util.SplitCSV(tmpStr))
				{
					string[] values = abilities.Split('|');
					if (values.Length >= 2)
					{
						int level;
						if (int.TryParse(values[1], out level))
						{
							Ability ability = SkillBase.GetAbility(values[0], level);
							if (ability != null && ability is RealmAbility)
							{
								// this enable realm abilities for Career Computing.
								m_realmAbilities.Add((RealmAbility)ability);
							}
						}
					}
				}
			}

			// Load dependent skills
			RefreshSpecDependantSkills(false);
			
			#endregion

			#region disable ability
			//Since we added all the abilities that this character has, let's now disable the disabled ones!
			tmpStr = character.DisabledAbilities;
			if (tmpStr != null && tmpStr.Length > 0)
			{
				foreach (string str in Util.SplitCSV(tmpStr))
				{
					string[] values = str.Split('|');
					if (values.Length >= 2)
					{
						string keyname = values[0];
						int duration;
						if (HasAbility(keyname) && int.TryParse(values[1], out duration))
						{
							DisableSkill(GetAbility(keyname), duration);
						}
						else if (log.IsErrorEnabled)
						{
							log.ErrorFormat("{0}: error in loading disabled abilities => '{1}'", Name, tmpStr);
						}
					}
				}
			}

			#endregion
			
			//Load the disabled spells
			tmpStr = character.DisabledSpells;
			if (!string.IsNullOrEmpty(tmpStr))
			{
				foreach (string str in Util.SplitCSV(tmpStr))
				{
					string[] values = str.Split('|');
					int spellid;
					int duration;
					if (values.Length >= 2 && int.TryParse(values[0], out spellid) && int.TryParse(values[1], out duration))
					{
						Spell sp = SkillBase.GetSpellByID(spellid);
						// disable
						if (sp != null)
							DisableSkill(sp, duration);
					}
					else if (log.IsErrorEnabled)
					{
						log.ErrorFormat("{0}: error in loading disabled spells => '{1}'", Name, tmpStr);
					}
				}
			}
		}
		
		public virtual void LoadClassSpecializations(bool sendMessages)
		{
			// Get this Attached Class Specialization from SkillBase.
			IDictionary<Specialization, int> careers = SkillBase.GetSpecializationCareer(CharacterClass.ID);
			
			// Remove All Trainable Specialization or "Career Spec" that aren't managed by This Data Career anymore
			var speclist = GetSpecList();
			var careerslist = careers.Keys.Select(k => k.KeyName.ToLower());
			foreach (var spec in speclist.Where(sp => sp.Trainable || !sp.AllowSave))
			{
				if (!careerslist.Contains(spec.KeyName.ToLower()))
					RemoveSpecialization(spec.KeyName);
			}
						
			// sort ML Spec depending on ML Line
			byte mlindex = 0;
			foreach (KeyValuePair<Specialization, int> constraint in careers)
			{
				if (constraint.Key is IMasterLevelsSpecialization)
				{
					if (mlindex != MLLine)
					{
						if (HasSpecialization(constraint.Key.KeyName))
							RemoveSpecialization(constraint.Key.KeyName);
						
						mlindex++;
						continue;
					}
					
					mlindex++;
					
					if (!MLGranted || MLLevel < 1)
					{
						continue;
					}
				}
				
				// load if the spec doesn't exists
				if (Level >= constraint.Value)
				{
					if (!HasSpecialization(constraint.Key.KeyName))
						AddSpecialization(constraint.Key, sendMessages);
				}
				else
				{
					if (HasSpecialization(constraint.Key.KeyName))
						RemoveSpecialization(constraint.Key.KeyName);
				}
			}
		}
		
		public override void SaveIntoDatabase()
		{
			try
			{
				// Ff this player is a GM always check and set the IgnoreStatistics flag
				if (Network.Account.PrivLevel > (uint)ePrivLevel.Player && DBCharacter.IgnoreStatistics == false)
				{
					DBCharacter.IgnoreStatistics = true;
				}

				SaveSkillsToCharacter();

				DBCharacter.PlayedTime = PlayedTime;  //We have to set the PlayedTime on the character before setting the LastPlayed
				DBCharacter.LastPlayed = DateTime.Now;

				DBCharacter.ActiveWeaponSlot = (byte)ActiveWeaponSlot;
				if (m_stuckFlag)
				{
					lock (LastUniquePositions)
					{
						DBCharacter.SetPosition(LastUniquePositions[LastUniquePositions.Length - 1]);
					}
				}
				GameServer.Database.SaveObject(DBCharacter);
				Inventory.SaveIntoDatabase(InternalID);

				DOLCharacters cachedCharacter = null;

				foreach (DOLCharacters accountChar in Network.Account.Characters)
				{
					if (accountChar.ObjectId == InternalID)
					{
						cachedCharacter = accountChar;
						break;
					}
				}

				if (cachedCharacter != null)
				{
					cachedCharacter = DBCharacter;
				}

				Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.SaveIntoDatabase.CharacterSaved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Error saving player {0}! - {1}", Name, e);
			}
		}
		
		public bool SetCharacterClass(CharacterClass charClass)
		{
			if (charClass.Equals(CharacterClass.None))
			{
				if (log.IsErrorEnabled) log.ErrorFormat($"Unknown CharacterClass has been set for Player {Name}.");
				return false;
			}
			
			CharacterClass = charClass;
			DBCharacter.Class = CharacterClass.ID;
			return true;
		}
		
		public GamePlayer(GameClient client, DOLCharacters dbChar)
			: base()
		{
			Wallet = new Wallet(this);
			mNetwork = client;
			mdbCharacter = dbChar;
			
			#region guild handling ================================================
			var guildid = client.Account.GuildID;
			if (guildid != null)
				m_guild = GuildMgr.GetGuildByGuildID(guildid);
			else
				m_guild = null;

			if (m_guild != null)
			{
				foreach (DBRank rank in m_guild.Ranks)
				{
					if (rank == null) continue;
					if (rank.RankLevel == DBCharacter.GuildRank)
					{
						m_guildRank = rank;
						break;
					}
				}

				m_guildName = m_guild.Name;
				m_guild.AddOnlineMember(this);
			}
			#endregion
			
			CreateInventory();
			GameEventManager.AddHandler(m_inventory, PlayerInventoryEvent.ItemEquipped, new GameEventHandler(OnItemEquipped));
			GameEventManager.AddHandler(m_inventory, PlayerInventoryEvent.ItemUnequipped, new GameEventHandler(OnItemUnequipped));
			GameEventManager.AddHandler(m_inventory, PlayerInventoryEvent.ItemBonusChanged, new GameEventHandler(OnItemBonusChanged));
			
			m_enteredGame = false;
			m_customDialogCallback = null;
			m_sitting = false;
			m_isWireframe = false;
			CharacterClass = CharacterClass.None;

			m_saveInDB = true;
			LoadFromDatabase(dbChar);			
		}
		
		protected virtual void CreateInventory()
		{
			m_inventory = new GamePlayerInventory(this);
		}
    }
}
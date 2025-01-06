using Logic.database.attribute;

namespace Logic.database.table;

	[DataTable(TableName = "Spell")]
	public class DBSpell : DataObject
	{
		protected int m_spellid;
		protected int m_effectid;
		protected int m_icon;
		protected string m_name;
		protected string m_description;
		protected string m_target = string.Empty;

		protected string m_spelltype = string.Empty;
		protected int m_range = 0;
		protected int m_radius = 0;
		protected double m_value = 0;
		protected double m_damage = 0;
		protected int m_damageType = 0;
		protected int m_concentration = 0;
		protected int m_duration = 0;
		protected int m_pulse = 0;
		protected int m_frequency = 0;
		protected int m_pulse_power = 0;
		protected int m_power = 0;
		protected double m_casttime = 0;
		protected int m_recastdelay = 0;
		protected int m_reshealth = 1;
		protected int m_resmana = 0;
		protected int m_lifedrain_return = 0;
		protected int m_amnesia_chance = 0;
		protected string m_message1 = string.Empty;
		protected string m_message2 = string.Empty;
		protected string m_message3 = string.Empty;
		protected string m_message4 = string.Empty;
		protected int m_instrumentRequirement;
		protected int m_spellGroup;
		protected int m_effectGroup;
		protected int m_subSpellID = 0;
		protected bool m_moveCast = false;
		protected bool m_uninterruptible = false;
		protected bool m_isfocus = false;
		protected int m_sharedtimergroup;
		protected string m_packageID = string.Empty;
		
		// warlock
		protected bool m_isprimary;
		protected bool m_issecondary;
		protected bool m_allowbolt;

		// tooltip
		protected ushort m_tooltipId;
		
		public DBSpell()
		{
			AllowAdd = false;
		}

		[DataElement(AllowDbNull = false, Unique = true)]
		public int SpellID
		{
			get => m_spellid;
			set => SetProperty(ref m_spellid, value);
		}

		[DataElement(AllowDbNull = false)]
		public int ClientEffect
		{
			get => m_effectid;
			set => SetProperty(ref m_effectid, value);     			
		}

		[DataElement(AllowDbNull = false)]
		public int Icon
		{
			get => m_icon;
			set => SetProperty(ref m_icon, value);     			
		}

		[DataElement(AllowDbNull = false)]
		public string Name
		{
			get => m_name;
			set => SetProperty(ref m_name, value);     			
		}

		[DataElement(AllowDbNull = false)]
		public string Description
		{
			get => m_description;
			set => SetProperty(ref m_description, value);			
		}

		[DataElement(AllowDbNull = false)]
		public string Target
		{
			get => m_target;
			set => SetProperty(ref m_target, value);
		}

		[DataElement(AllowDbNull = false)]
		public int Range
		{
			get => m_range;
			set => SetProperty(ref m_range, value);			
		}

		[DataElement(AllowDbNull = false)]
		public int Power
		{
			get => m_power;
			set => SetProperty(ref m_power, value);			
		}

		[DataElement(AllowDbNull = false)]
		public double CastTime
		{
			get => m_casttime;
			set => SetProperty(ref m_casttime, value);					
		}

		[DataElement(AllowDbNull = false)]
		public double Damage
		{
			get => m_damage;
			set => SetProperty(ref m_damage, value);				
		}

		[DataElement(AllowDbNull = false)]
		public int DamageType
		{
			get => m_damageType;
			set => SetProperty(ref m_damageType, value);				
		}

		[DataElement(AllowDbNull = true)]
		public string Type
		{
			get => m_spelltype;
			set => SetProperty(ref m_spelltype, value);				
		}

		[DataElement(AllowDbNull = false)]
		public int Duration
		{
			get => m_duration;
			set => SetProperty(ref m_duration, value);
		}

		[DataElement(AllowDbNull = false)]
		public int Frequency
		{
			get => m_frequency;
			set => SetProperty(ref m_frequency, value);
		}

		[DataElement(AllowDbNull = false)]
		public int Pulse
		{
			get => m_pulse;
			set => SetProperty(ref m_pulse, value);
		}

		[DataElement(AllowDbNull = false)]
		public int PulsePower
		{
			get => m_pulse_power;
			set => SetProperty(ref m_pulse_power, value);
		}

		[DataElement(AllowDbNull = false)]
		public int Radius
		{
			get => m_radius;
			set => SetProperty(ref m_radius, value);
		}

		[DataElement(AllowDbNull = false)]
		public int RecastDelay
		{
			get => m_recastdelay;
			set => SetProperty(ref m_recastdelay, value);
		}

		[DataElement(AllowDbNull = false)]
		public int ResurrectHealth
		{
			get => m_reshealth;
			set => SetProperty(ref m_reshealth, value);
		}

		[DataElement(AllowDbNull = false)]
		public int ResurrectMana
		{
			get => m_resmana;
			set => SetProperty(ref m_resmana, value);
		}

		[DataElement(AllowDbNull = false)]
		public double Value
		{
			get => m_value;
			set => SetProperty(ref m_value, value);
		}

		[DataElement(AllowDbNull = false)]
		public int Concentration
		{
			get => m_concentration;
			set => SetProperty(ref m_concentration, value);
		}

		[DataElement(AllowDbNull = false)]
		public int LifeDrainReturn
		{
			get => m_lifedrain_return;
			set => SetProperty(ref m_lifedrain_return, value);
		}

		[DataElement(AllowDbNull = false)]
		public int AmnesiaChance
		{
			get => m_amnesia_chance;
			set => SetProperty(ref m_amnesia_chance, value);
		}

		[DataElement(AllowDbNull = true)]
		public string Message1
		{
			get => m_message1;
			set => SetProperty(ref m_message1, value);
		}

		[DataElement(AllowDbNull = true)]
		public string Message2
		{
			get => m_message2;
			set => SetProperty(ref m_message2, value);
		}

		[DataElement(AllowDbNull = true)]
		public string Message3
		{
			get => m_message3;
			set => SetProperty(ref m_message3, value);
		}

		[DataElement(AllowDbNull = true)]
		public string Message4
		{
			get => m_message4;
			set => SetProperty(ref m_message4, value);
		}

		[DataElement(AllowDbNull = false)]
		public int InstrumentRequirement
		{
			get => m_instrumentRequirement;
			set => SetProperty(ref m_instrumentRequirement, value);
		}

		[DataElement(AllowDbNull = false)]
		public int SpellGroup
		{
			get => m_spellGroup;
			set => SetProperty(ref m_spellGroup, value);
		}

		[DataElement(AllowDbNull = false)]
		public int EffectGroup
		{
			get => m_effectGroup;
			set => SetProperty(ref m_effectGroup, value);
		}

		//Multiple spells
		[DataElement(AllowDbNull = false)]
		public int SubSpellID
		{
			get => m_subSpellID;
			set => SetProperty(ref m_subSpellID, value);
		}

		[DataElement(AllowDbNull = false)]
		public bool MoveCast
		{
			get => m_moveCast;
			set => SetProperty(ref m_moveCast, value);
		}

		[DataElement(AllowDbNull = false)]
		public bool Uninterruptible
		{
			get => m_uninterruptible;
			set => SetProperty(ref m_uninterruptible, value);
		}
		
		[DataElement(AllowDbNull = false)]
		public bool IsFocus
		{
			get => m_isfocus;
			set => SetProperty(ref m_isfocus, value);
		}
		
		[DataElement(AllowDbNull = false)]
		public int SharedTimerGroup
		{
			get => m_sharedtimergroup;
			set => SetProperty(ref m_sharedtimergroup, value);
		}

		#region warlock
		[DataElement(AllowDbNull = false)]
		public bool IsPrimary
		{
			get => m_isprimary;
			set => SetProperty(ref m_isprimary, value);
		}
		
		[DataElement(AllowDbNull = false)]
		public bool IsSecondary
		{
			get => m_issecondary;
			set => SetProperty(ref m_issecondary, value);
		}
		
		[DataElement(AllowDbNull = false)]
		public bool AllowBolt
		{
			get => m_allowbolt;
			set => SetProperty(ref m_allowbolt, value);
		}
		
		[DataElement(AllowDbNull = true)]
		public string PackageID
		{
			get => m_packageID;
			set => SetProperty(ref m_packageID, value);
		}
		#endregion

		[DataElement(AllowDbNull = false)]
		public ushort TooltipId
		{
			get => m_tooltipId;
			set => SetProperty(ref m_tooltipId, value);
		}
		
		[Relation(LocalField = nameof( SpellID ), RemoteField = nameof( DBSpellXCustomValues.SpellID ), AutoLoad = true, AutoDelete=true)]
		public DBSpellXCustomValues[] CustomValues;
	}
	
	
	/// <summary>
	/// Spell Custom Values Table containing entries linked to spellID.
	/// </summary>
	[DataTable(TableName = "SpellXCustomValues")]
	public class DBSpellXCustomValues : CustomParam
	{
		private int m_spellID;
		
		/// <summary>
		/// Spell Table SpellID Reference
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int SpellID {
			get => m_spellID;
			set => SetProperty(ref m_spellID, value);
		}
		
		/// <summary>
		/// Create new instance of <see cref="DBSpellXCustomValues"/> linked to Spell ID.
		/// </summary>
		/// <param name="SpellID">Spell ID</param>
		/// <param name="KeyName">Key Name</param>
		/// <param name="Value">Value</param>
		public DBSpellXCustomValues(int SpellID, string KeyName, string Value)
			: base(KeyName, Value)
		{
			this.SpellID = SpellID;
		}
		
		/// <summary>
		/// Create new instance of <see cref="DBSpellXCustomValues"/>
		/// </summary>
		public DBSpellXCustomValues()
		{
		}
			
	}
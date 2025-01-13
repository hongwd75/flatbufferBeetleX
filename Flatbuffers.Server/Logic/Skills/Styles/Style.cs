using Game.Logic.Inventory;
using Game.Logic.Skills;
using Logic.database.table;

namespace Game.Logic.Styles;

public class Style : Skill
	{
		/// <summary>
		/// The opening type of a style
		/// </summary>
		public enum eOpening : int
		{
			/// <summary>
			/// Offensive opening, depending on the attacker's actions
			/// </summary>
			Offensive = 0,
			/// <summary>
			/// Defensive opening, depending on the enemy's action
			/// </summary>
			Defensive = 1,
			/// <summary>
			/// Positional opening, depending on the attacker to target position
			/// </summary>
			Positional = 2
		}

		/// <summary>
		/// The opening positions if the style is a position based style
		/// </summary>
		public enum eOpeningPosition : int
		{
			/// <summary>
			/// Towards back of the target
			/// </summary>
			Back = 0,
			/// <summary>
			/// Towards the side of the target
			/// </summary>
			Side = 1,
			/// <summary>
			/// Towards the front of the target
			/// </summary>
			Front = 2
		}

		/// <summary>
		/// The required attack result of the style 
		/// </summary>
		public enum eAttackResultRequirement : int
		{
			/// <summary>
			/// Any attack result is fine
			/// </summary>
			Any = 0,
			/// <summary>
			/// A miss is required
			/// </summary>
			Miss = 1,
			/// <summary>
			/// A hit is required
			/// </summary>
			Hit = 2,
			/// <summary>
			/// A parry is required
			/// </summary>
			Parry = 3,
			/// <summary>
			/// A block is required
			/// </summary>
			Block = 4,
			/// <summary>
			/// An evade is required
			/// </summary>
			Evade = 5,
			/// <summary>
			/// A fumble is required
			/// </summary>
			Fumble = 6,
			/// <summary>
			/// A style is required
			/// </summary>
			Style = 7
		}

		/// <summary>
		/// Special weapon type requirements
		/// </summary>
		public abstract class SpecialWeaponType
		{
			/// <summary>
			/// Both hands should be holding weapons to use style.
			/// Shield is not a weapon in this case.
			/// </summary>
			public const int DualWield = 1000;
			/// <summary>
			/// Stlye can be used with 1h, 2h, dw.
			/// Used for Critical Strike line.
			/// </summary>
			public const int AnyWeapon = 1001;
		}

		/// <summary>
		/// The database style object, used to retrieve information for this object
		/// </summary>
		protected DBStyle baseStyle = null;

		/// <summary>
		/// Constructs a new Style object based on a database Style object
		/// </summary>
		/// <param name="style">The database style object this object is based on</param>
		public Style(DBStyle style)
			: base(style.Name, style.ID, (ushort)style.Icon, style.SpecLevelRequirement, style.StyleID)
		{
			baseStyle = style;
		}

        public int ClassID
        {
            get { return baseStyle.ClassId; }
        }

		/// <summary>
		/// (readonly)(procs) The list of procs available for this style
		/// </summary>
		public IList<Tuple<Spell, int, int>> Procs
		{
			get { return SkillBase.GetStyleProcsByID(this); }
		}
		
		/// <summary>
		/// (readonly) The Specialization's name required to execute this style
		/// </summary>
		public string Spec
		{
			get { return baseStyle.SpecKeyName; }
		}

		/// <summary>
		/// (readonly) The Specialization's level required to execute this style
		/// </summary>
		public int SpecLevelRequirement
		{
			get { return baseStyle.SpecLevelRequirement; }
		}

		/// <summary>
		/// (readonly) The fatique cost of this style in % of player's total fatique
		/// This cost will be modified by weapon speed, realm abilities and magic effects
		/// </summary>
		public int EnduranceCost
		{
			get { return baseStyle.EnduranceCost; }
		}

		/// <summary>
		/// (readonly) Stealth requirement of this style
		/// </summary>
		public bool StealthRequirement
		{
			get { return baseStyle.StealthRequirement; }
		}

		/// <summary>
		/// (readonly) The opening type of this style
		/// </summary>
		public eOpening OpeningRequirementType
		{
			get { return (eOpening)baseStyle.OpeningRequirementType; }
		}

		/// <summary>
		/// (readonly) Depending on the OpeningRequirementType.
		/// If the style is a offensive opened style, this 
		/// holds the style id the attacker is required to
		/// execute before this style. 
		/// If the style is a defensive opened style, this
		/// holds the style id the defender is required to
		/// execute before the attacker can use this style.
		/// (values other than 0 require a nonspecific style)
		/// If the style is a position opened style, this
		/// holds the position requirement.
		/// </summary>
		public int OpeningRequirementValue
		{
			get { return baseStyle.OpeningRequirementValue; }
		}

		/// <summary>
		/// (readonly) The attack result required from 
		/// attacker(offensive style) or defender(defensive style)
		/// </summary>
		public eAttackResultRequirement AttackResultRequirement
		{
			get { return (eAttackResultRequirement)baseStyle.AttackResultRequirement; }
		}

		/// <summary>
		/// (readonly) The type of weapon required to execute this style.
		/// If not one of SpecialWeaponType then eObjectType is used.
		/// </summary>
		public int WeaponTypeRequirement
		{
			get { return baseStyle.WeaponTypeRequirement; }
		}

		/// <summary>
		/// (readonly) The growth offset of the style growth function
		/// </summary>
		public double GrowthOffset
		{
			get { return baseStyle.GrowthOffset; }
		}

		/// <summary>
		/// (readonly) The growth rate of the style
		/// </summary>
		public double GrowthRate
		{
			get { return baseStyle.GrowthRate; }
		}

		/// <summary>
		/// (readonly) The bonus to hit if this style get's executed successfully
		/// </summary>
		public int BonusToHit
		{
			get { return baseStyle.BonusToHit; }
		}

		/// <summary>
		/// (readonly) The bonus to defense if this style get's executed successfully
		/// </summary>
		public int BonusToDefense
		{
			get { return baseStyle.BonusToDefense; }
		}

		/// <summary>
		/// (readonly) The type of this skill, always returns eSkillPage.Styles
		/// </summary>
		public override eSkillPage SkillType
		{
			get { return eSkillPage.Styles; }
		}

		/// <summary>
		/// (readonly) The animation ID for 2h weapon styles
		/// </summary>
		public int TwoHandAnimation
		{
			get { return baseStyle.TwoHandAnimation; }
		}

		/// <summary>
		/// (readonly) (procs) Tell if the proc should be select randomly
		/// </summary>
		public bool RandomProc
		{
			get { return baseStyle.RandomProc; }
		}

		public eArmorSlot ArmorHitLocation
		{
			get { return (eArmorSlot)baseStyle.ArmorHitLocation; }
		}

		/// <summary>
		/// Gets name of required weapon type
		/// </summary>
		/// <returns>name of required weapon type</returns>
		public virtual string GetRequiredWeaponName()
		{
			switch (WeaponTypeRequirement)
			{
				case SpecialWeaponType.DualWield:
					// style line spec name
					Specialization dwSpec = SkillBase.GetSpecialization(Spec);
					if (dwSpec == null) return "unknown DW spec";
					else return dwSpec.Name;

				case SpecialWeaponType.AnyWeapon:
					return "Any";

				default:
					// spec name needed to use weapon type
					string specKeyName = SkillBase.ObjectTypeToSpec((eObjectType)WeaponTypeRequirement);
					if (specKeyName == null)
						return "unknown weapon type";
					Specialization spec = SkillBase.GetSpecialization(specKeyName);
					return spec.Name;
			}
		}


		public override Skill Clone()
		{
			return (Style)MemberwiseClone();
		}
	}
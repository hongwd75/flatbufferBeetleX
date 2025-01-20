using Game.Logic.Inventory;
using Game.Logic.Language;
using Game.Logic.network;
using Game.Logic.PropertyCalc;
using Game.Logic.Skills;
using Game.Logic.Utils;
using Game.Logic.World;
using Logic.database.table;

namespace Game.Logic;

public enum ePrivLevel : uint
{
    Player = 1,
    GM = 2,
    Admin = 3,
}


[Flags]
public enum ePropertyType : ushort
{
    Focus = 1,
    Resist = 1 << 1,
    Skill = 1 << 2,
    SkillMeleeWeapon = 1 << 3,
    SkillMagical = 1 << 4,
    SkillDualWield = 1 << 5,
    SkillArchery = 1 << 6,
    ResistMagical = 1 << 7,
    Albion = 1 << 8,
    Midgard = 1 << 9,
    Hibernia = 1 << 10,
    Common = 1 << 11,
    CapIncrease = 1 << 12,
}

public enum eDamageType : byte
{
    _FirstResist = 0,
    Natural = 0,
    Crush = 1,
    Slash = 2,
    Thrust = 3,

    Body = 10,
    Cold = 11,
    Energy = 12,
    Heat = 13,
    Matter = 14,
    Spirit = 15,
    _LastResist = 15,
    /// <summary>
    /// Damage is from a GM via a command
    /// </summary>
    GM = 254,
    /// <summary>
    /// Player is taking falling damage
    /// </summary>
    Falling = 255,
}

public enum eGender : byte
{
    Neutral = 0,
    Male = 1,
    Female = 2
}

public enum eRace : byte
{
    Unknown = 0,
    Briton = 1,
    Celt = 11,
    Troll = 21,
    max = 22
}

public enum eLivingModel : ushort
{
    None = 0,
    #region AlbionClassModels
    BritonMale = 1,
    BritonFemale = 2,
    #endregion
    #region MidgardClassModels
    TrollMale = 3,
    TrollFemale = 4,
    #endregion
    #region HiberniaClassModels
    CeltMale = 5,
    CeltFemale = 6,
    #endregion
    #region Hastener
    AlbionHastener = 100,
    MidgardHastener = 101,
    HiberniaHastener = 102,
    #endregion Hastener
}

public enum eCharacterClass : byte
{
    Unknown = 0,
    //base classes
    Acolyte = 1,
    Mage = 2,
    Fighter = 3,
    
    Naturalist = 4,
    Magician = 5,
    Guardian = 6,

    Seer = 7,
    Viking = 8,
    Mystic = 9,
    
    //alb classes
    Armsman = 10,
    Cleric = 11,
    Wizard = 12,

    //mid classes
    Warrior = 13,
    Healer = 14,
    Runemaster = 15,

    //hib classes
    Hero = 16,
    Druid = 17,
    Eldritch = 18,
}

/////
public static class GlobalConstants
	{
		private static readonly Dictionary<GameLiving.eAttackResult, byte> AttackResultByte = new Dictionary<GameLiving.eAttackResult, byte>()
	    {
			{GameLiving.eAttackResult.Missed, 0},
			{GameLiving.eAttackResult.Parried, 1},
			{GameLiving.eAttackResult.Blocked, 2},
			{GameLiving.eAttackResult.Evaded, 3},
			{GameLiving.eAttackResult.Fumbled, 4},
			{GameLiving.eAttackResult.HitUnstyled, 10},
			{GameLiving.eAttackResult.HitStyle, 11},
			{GameLiving.eAttackResult.Any, 20},
	    };
		
		public static byte GetAttackResultByte(GameLiving.eAttackResult attResult)
		{
			if (AttackResultByte.ContainsKey(attResult))
			{
				return AttackResultByte[attResult];
			}
			
			return 0;
		}
		
		public static bool IsExpansionEnabled(int expansion)
		{
			bool enabled = true;
			foreach (string ex in Util.SplitCSV(ServerProperties.Properties.DISABLED_EXPANSIONS, true))
			{
				int exNum = 0;
				if (int.TryParse(ex, out exNum))
				{
					if (exNum == expansion)
					{
						enabled = false;
						break;
					}
				}
			}

			return enabled;
		}


		public static string StatToName(eStat stat)
		{
			switch (stat)
			{
				case eStat.STR:
					return "Strength";
				case eStat.DEX:
					return "Dexterity";
				case eStat.CON:
					return "Constitution";
				case eStat.QUI:
					return "Quickness";
				case eStat.INT:
					return "Intelligence";
				case eStat.PIE:
					return "Piety";
				case eStat.EMP:
					return "Empathy";
				case eStat.CHR:
					return "Charisma";
			}

			return "Unknown";
		}

		/// <summary>
		/// Check an Object_Type to determine if it's a Bow weapon
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public static bool IsBowWeapon(eObjectType objectType)
		{
			return (objectType == eObjectType.CompositeBow || objectType == eObjectType.Longbow || objectType == eObjectType.RecurvedBow);
		}
		/// <summary>
		/// Check an Object_Type to determine if it's a weapon
		/// </summary>
		/// <param name="objectTypeID"></param>
		/// <returns></returns>
		public static bool IsWeapon(int objectTypeID)
		{
			if ((objectTypeID >= 1 && objectTypeID <= 28) || objectTypeID == (int)eObjectType.Shield) return true;
			return false;
		}
		/// <summary>
		/// Check an Object_Type to determine if it's armor
		/// </summary>
		/// <param name="objectTypeID"></param>
		/// <returns></returns>
		public static bool IsArmor(int objectTypeID)
		{
			if (objectTypeID >= 32 && objectTypeID <= 38) return true;
			return false;
		}
		/// <summary>
		/// Offensive, Defensive, or Positional
		/// </summary>
		public static string StyleOpeningTypeToName(int openingType)
		{
			return Enum.GetName(typeof(Styles.Style.eOpening), openingType);
		}
		/// <summary>
		/// Position, Back, Side, Front
		/// </summary>
		public static string StyleOpeningPositionToName(int openingRequirement)
		{
			return Enum.GetName(typeof(Styles.Style.eOpeningPosition), openingRequirement);
		}
		/// <summary>
		/// Attack Result. Any, Miss, Hit, Parry, Block, Evade, Fumble, Style.
		/// </summary>
		public static string StyleAttackResultToName(int attackResult)
		{
			return Enum.GetName(typeof(Styles.Style.eAttackResultRequirement), attackResult);
		}

		public static string InstrumentTypeToName(int instrumentTypeID)
		{
			return Enum.GetName(typeof(eInstrumentType), instrumentTypeID);
		}

		public static string AmmunitionTypeToDamageName(int ammutype)
		{
			ammutype &= 0x3;
			switch (ammutype)
			{
					case 1: return "medium";
					case 2: return "heavy";
					case 3: return "X-heavy";
			}
			return "light";
		}

		public static string AmmunitionTypeToRangeName(int ammutype)
		{
			ammutype = (ammutype >> 2) & 0x3;
			switch (ammutype)
			{
					case 1: return "medium";
					case 2: return "long";
					case 3: return "X-long";
			}
			return "short";
		}

		public static string AmmunitionTypeToAccuracyName(int ammutype)
		{
			ammutype = (ammutype >> 4) & 0x3;
			switch (ammutype)
			{
					case 1: return "normal";
					case 2: return "improved";
					case 3: return "enhanced";
			}
			return "reduced";
		}

		public static string ShieldTypeToName(int shieldTypeID)
		{
			return Enum.GetName(typeof(ShieldLevel), shieldTypeID);
		}

		public static string ArmorLevelToName(int armorLevel, eRealm realm)
		{
			switch (realm)
			{
				case eRealm.Albion:
					{
						switch (armorLevel)
						{
								case ArmorLevel.Cloth: return "cloth";
								case ArmorLevel.Chain: return "chain";
								case ArmorLevel.Leather: return "leather";
								case ArmorLevel.Plate: return "plate";
								case ArmorLevel.Studded: return "studded";
								default: return "undefined";
						}
					}
				case eRealm.Midgard:
					{
						switch (armorLevel)
						{
								case ArmorLevel.Cloth: return "cloth";
								case ArmorLevel.Chain: return "chain";
								case ArmorLevel.Leather: return "leather";
								case ArmorLevel.Studded: return "studded";
								default: return "undefined";
						}
					}
				case eRealm.Hibernia:
					{
						switch (armorLevel)
						{
								case ArmorLevel.Cloth: return "cloth";
								case ArmorLevel.Scale: return "scale";
								case ArmorLevel.Leather: return "leather";
								case ArmorLevel.Reinforced: return "reinforced";
								default: return "undefined";
						}
					}
					default: return "undefined";
			}
		}

		public static string WeaponDamageTypeToName(int weaponDamageTypeID)
		{
			return Enum.GetName(typeof(eWeaponDamageType), weaponDamageTypeID);
		}

		public static string NameToShortName(string name)
		{
			string[] values = name.Trim().ToLower().Split(' ');
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i].Length == 0) continue;
				if (i > 0 && values[i] == "of")
					return values[i - 1];
			}
			return values[values.Length - 1];
		}

		public static string ItemHandToName(int handFlag)
		{
			if (handFlag == 1) return "twohanded";
			if (handFlag == 2) return "lefthand";
			return "both";
		}

		public static string ObjectTypeToName(int objectTypeID)
		{
			switch (objectTypeID)
			{
					case 0: return "generic (item)";
					case 1: return "generic (weapon)";
					case 2: return "crushing (weapon)";
					case 3: return "slashing (weapon)";
					case 4: return "thrusting (weapon)";
					case 5: return "fired (weapon)";
					case 6: return "twohanded (weapon)";
					case 7: return "polearm (weapon)";
					case 8: return "staff (weapon)";
					case 9: return "longbow (weapon)";
					case 10: return "crossbow (weapon)";
					case 11: return "sword (weapon)";
					case 12: return "hammer (weapon)";
					case 13: return "axe (weapon)";
					case 14: return "spear (weapon)";
					case 15: return "composite bow (weapon)";
					case 16: return "thrown (weapon)";
					case 17: return "left axe (weapon)";
					case 18: return "recurve bow (weapon)";
					case 19: return "blades (weapon)";
					case 20: return "blunt (weapon)";
					case 21: return "piercing (weapon)";
					case 22: return "large (weapon)";
					case 23: return "celtic spear (weapon)";
					case 24: return "flexible (weapon)";
					case 25: return "hand to hand (weapon)";
					case 26: return "scythe (weapon)";
					case 27: return "fist wraps (weapon)";
					case 28: return "mauler staff (weapon)";
					case 31: return "generic (armor)";
					case 32: return "cloth (armor)";
					case 33: return "leather (armor)";
					case 34: return "studded leather (armor)";
					case 35: return "chain (armor)";
					case 36: return "plate (armor)";
					case 37: return "reinforced (armor)";
					case 38: return "scale (armor)";
					case 41: return "magical (item)";
					case 42: return "shield (armor)";
					case 43: return "arrow (item)";
					case 44: return "bolt (item)";
					case 45: return "instrument (item)";
					case 46: return "poison (item)";
					case 47: return "alchemy tincture";
					case 48: return "spellcrafting gem";
					case 49: return "garden object";
					case 50: return "house wall object";
					case 51: return "house floor object";
					case 53: return "house npc";
					case 54: return "house vault";
					case 55: return "house crafting object";
					case 68: return "house bindstone";
			}
			return "unknown (item)";
		}

		//This method translates an InventoryTypeID to a string
		public static string SlotToName(int slotID)
		{
			switch (slotID)
			{
				case 0x0A: return "righthand";
				case 0x0B: return "lefthand";
				case 0x0C: return "twohanded";
				case 0x0D: return "distance";
				case 0x15: return "head";
				case 0x16: return "hand";
				case 0x17: return "feet";
				case 0x18: return "jewel";
				case 0x19: return "torso";
				case 0x1A: return "cloak";
				case 0x1B: return "legs";
				case 0x1C: return "arms";
				case 0x1D: return "neck";
				case 0x20: return "belt";
				case 0x21: return "leftbracer";
				case 0x22: return "rightbracer";
				case 0x23: return "leftring";
				case 0x24: return "rightring";
				case 0x25: return "mythirian";
				case 96: return "leftfront saddlebag";
				case 97: return "rightfront saddlebag";
				case 98: return "leftrear saddlebag";
				case 99: return "rightrear saddlebag";
			}
			return "generic inventory";
		}

		//This method translates a string to an InventorySlotID
		public static byte NameToSlot(string name)
		{
			switch (name)
			{
					//Horses
					case "mount": return 0xA9;
					//Righthand Weapon Type
					case "righthand": return 0x0A;
					case "right": return 0x0A;
					case "ri": return 0x0A;

					//Lefthand Weapon Type
					case "lefthand": return 0x0B;
					case "left": return 0x0B;
					case "lef": return 0x0B;

					//Twohanded Weapon Type
					case "twohanded": return 0x0C;
					case "two": return 0x0C;
					case "tw": return 0x0C;

					//Distance Weapon Type
					case "distance": return 0x0D;
					case "dist": return 0x0D;
					case "di": return 0x0D;
					case "bow": return 0x0D;
					case "crossbow": return 0x0D;
					case "longbow": return 0x0D;
					case "throwing": return 0x0D;
					case "thrown": return 0x0D;
					case "fire": return 0x0D;
					case "firing": return 0x0D;

					//Head Armor Type
					case "head": return 0x15;
					case "helm": return 0x15;
					case "he": return 0x15;

					//Hand Armor Type
					case "hands": return 0x16;
					case "hand": return 0x16;
					case "ha": return 0x16;
					case "gloves": return 0x16;
					case "glove": return 0x16;
					case "gl": return 0x16;

					//Boot Armor Type
					case "boots": return 0x17;
					case "boot": return 0x17;
					case "boo": return 0x17;
					case "feet": return 0x17;
					case "fe": return 0x17;
					case "foot": return 0x17;
					case "fo": return 0x17;

					//Jewel Type
					case "jewels": return 0x18;
					case "jewel": return 0x18;
					case "je": return 0x18;
					case "j": return 0x18;
					case "gems": return 0x18;
					case "gem": return 0x18;
					case "gemstone": return 0x18;
					case "stone": return 0x18;

					//Body Armor Type
					case "torso": return 0x19;
					case "to": return 0x19;
					case "body": return 0x19;
					case "bod": return 0x19;
					case "robes": return 0x19;
					case "robe": return 0x19;
					case "ro": return 0x19;

					//Cloak Armor Type
					case "cloak": return 0x1A;
					case "cloa": return 0x1A;
					case "clo": return 0x1A;
					case "cl": return 0x1A;
					case "cape": return 0x1A;
					case "ca": return 0x1A;
					case "gown": return 0x1A;
					case "mantle": return 0x1A;
					case "ma": return 0x1A;
					case "shawl": return 0x1A;

					//Leg Armor Type
					case "legs": return 0x1B;
					case "leg": return 0x1B;

					//Arms Armor Type
					case "arms": return 0x1C;
					case "arm": return 0x1C;
					case "ar": return 0x1C;

					//Neck Armor Type
					case "neck": return 0x1D;
					case "ne": return 0x1D;
					case "scruff": return 0x1D;
					case "nape": return 0x1D;
					case "throat": return 0x1D;
					case "necklace": return 0x1D;
					case "necklet": return 0x1D;

					//Belt Armor Type
					case "belt": return 0x20;
					case "b": return 0x20;
					case "girdle": return 0x20;
					case "waistbelt": return 0x20;

					//Left Bracers Type
					case "leftbracers": return 0x21;
					case "leftbracer": return 0x21;
					case "leftbr": return 0x21;
					case "lbracers": return 0x21;
					case "lbracer": return 0x21;
					case "leb": return 0x21;
					case "lbr": return 0x21;
					case "lb": return 0x21;

					//Right Bracers Type
					case "rightbracers": return 0x22;
					case "rightbracer": return 0x22;
					case "rightbr": return 0x22;
					case "rbracers": return 0x22;
					case "rbracer": return 0x22;
					case "rib": return 0x22;
					case "rbr": return 0x22;
					case "rb": return 0x22;

					//Left Ring Type
					case "leftrings": return 0x23;
					case "leftring": return 0x23;
					case "leftr": return 0x23;
					case "lrings": return 0x23;
					case "lring": return 0x23;
					case "lri": return 0x23;
					case "ler": return 0x23;
					case "lr": return 0x23;

					//Right Ring Type
					case "rightrings": return 0x24;
					case "rightring": return 0x24;
					case "rightr": return 0x24;
					case "rrings": return 0x24;
					case "rring": return 0x24;
					case "rri": return 0x24;
					case "rir": return 0x24;
					case "rr": return 0x24;

					//Mythirians
					case "myth": return 0x25;
					case "mythirian": return 0x25;
					case "mythirians": return 0x25;
			}
			return 0x00;
		}
		public static string RealmToName(eRealm realm)
		{
			switch (realm)
			{
					case eRealm.None: return "None";
					case eRealm.Albion: return "Albion";
					case eRealm.Midgard: return "Midgard";
					case eRealm.Hibernia: return "Hibernia";
					default: return "";
			}
		}
		public static int EmblemOfRealm(eRealm realm)
		{
			switch (realm)
			{
					case eRealm.None: return 0;
					case eRealm.Albion: return 464;
					case eRealm.Midgard: return 465;
					case eRealm.Hibernia: return 466;
					default: return 0;
			}
		}

		public static string PropertyToName(eProperty property)
		{
			switch (property)
			{
					case eProperty.Strength: return "Strength";
					case eProperty.Dexterity: return "Dexterity";
					case eProperty.Constitution: return "Constitution";
					case eProperty.Quickness: return "Quickness";
					case eProperty.Intelligence: return "Intelligence";
					case eProperty.Piety: return "Piety";
					case eProperty.Empathy: return "Empathy";
					case eProperty.Charisma: return "Charisma";
					case eProperty.Resist_Body: return "Body Resist";
					case eProperty.Resist_Cold: return "Cold Resist";
					case eProperty.Resist_Crush: return "Crush Resist";
					case eProperty.Resist_Energy: return "Energy Resist";
					case eProperty.Resist_Heat: return "Heat Resist";
					case eProperty.Resist_Matter: return "Matter Resist";
					case eProperty.Resist_Slash: return "Slash Resist";
					case eProperty.Resist_Spirit: return "Spirit Resist";
					case eProperty.Resist_Thrust: return "Thrust Resist";
					case eProperty.Resist_Natural: return "Essence Resist";
					default: return "not implemented";
			}
		}

		public static string DamageTypeToName(eDamageType damage)
		{
			switch (damage)
			{
					case eDamageType.Body: return "Body";
					case eDamageType.Cold: return "Cold";
					case eDamageType.Crush: return "Crush";
					case eDamageType.Energy: return "Energy";
					case eDamageType.Falling: return "Falling";
					case eDamageType.Heat: return "Heat";
					case eDamageType.Matter: return "Matter";
					case eDamageType.Natural: return "Natural";
					case eDamageType.Slash: return "Slash";
					case eDamageType.Spirit: return "Spirit";
					case eDamageType.Thrust: return "Thrust";
					default: return "unknown damagetype " + damage.ToString();
			}
		}

		public static string CraftLevelToCraftTitle(GameClient client, int craftLevel)
		{
			switch ((int)(craftLevel / 100))
			{
                case 0: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Helper");
                case 1: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.JuniorApprentice");
                case 2: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Apprentice");
                case 3: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Neophyte");
                case 4: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Assistant");
                case 5: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Junior");
                case 6: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Journeyman");
                case 7: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Senior");
                case 8: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Master");
                case 9: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Grandmaster");
                case 10: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Legendary");
                case 11: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.LegendaryGrandmaster");
			}
			if (craftLevel > 1100)
			{
                return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.LegendaryGrandmaster");
			}
			return "";
		}

		public static eRealm GetBonusRealm(eProperty bonus)
		{
			if (SkillBase.CheckPropertyType(bonus, ePropertyType.Albion))
				return eRealm.Albion;
			if (SkillBase.CheckPropertyType(bonus, ePropertyType.Midgard))
				return eRealm.Midgard;
			if (SkillBase.CheckPropertyType(bonus, ePropertyType.Hibernia))
				return eRealm.Hibernia;
			return eRealm.None;
		}

		public static eRealm[] GetItemTemplateRealm(ItemTemplate item)
		{
			switch ((eObjectType)item.Object_Type)
			{
					//Albion
				case eObjectType.CrushingWeapon:
				case eObjectType.SlashingWeapon:
				case eObjectType.ThrustWeapon:
				case eObjectType.TwoHandedWeapon:
				case eObjectType.PolearmWeapon:
				case eObjectType.Staff:
				case eObjectType.Longbow:
				case eObjectType.Crossbow:
				case eObjectType.Flexible:
				case eObjectType.Plate:
				case eObjectType.Bolt:
					return new eRealm[] { eRealm.Albion };

					//Midgard
				case eObjectType.Sword:
				case eObjectType.Hammer:
				case eObjectType.Axe:
				case eObjectType.Spear:
				case eObjectType.CompositeBow:
				case eObjectType.Thrown:
				case eObjectType.LeftAxe:
				case eObjectType.HandToHand:
					return new eRealm[] { eRealm.Midgard };

					//Hibernia
				case eObjectType.Fired:
				case eObjectType.RecurvedBow:
				case eObjectType.Blades:
				case eObjectType.Blunt:
				case eObjectType.Piercing:
				case eObjectType.LargeWeapons:
				case eObjectType.CelticSpear:
				case eObjectType.Scythe:
				case eObjectType.Reinforced:
				case eObjectType.Scale:
					return new eRealm[] { eRealm.Hibernia };

					//Special
				case eObjectType.Studded:
				case eObjectType.Chain:
					return new eRealm[] { eRealm.Albion, eRealm.Midgard };

				case eObjectType.Instrument:
					return new eRealm[] { eRealm.Albion, eRealm.Hibernia };

					//Common Armor
				case eObjectType.Cloth:
				case eObjectType.Leather:
					//Misc
				case eObjectType.GenericItem:
				case eObjectType.GenericWeapon:
				case eObjectType.GenericArmor:
				case eObjectType.Magical:
				case eObjectType.Shield:
				case eObjectType.Arrow:
				case eObjectType.Poison:
				case eObjectType.AlchemyTincture:
				case eObjectType.SpellcraftGem:
				case eObjectType.GardenObject:
				case eObjectType.SiegeBalista:
				case eObjectType.SiegeCatapult:
				case eObjectType.SiegeCauldron:
				case eObjectType.SiegeRam:
				case eObjectType.SiegeTrebuchet:
					break;
			}

			eRealm realm = eRealm.None;

			if (item.Bonus1Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus1Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus2Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus2Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus3Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus3Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus4Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus4Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus5Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus5Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus6Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus6Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus7Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus7Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus8Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus8Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus9Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus9Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus10Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus10Type)) != eRealm.None)
				return new eRealm[] { realm };

			return new eRealm[] { realm };

		}

		public static byte GetSpecToInternalIndex(string name)
		{
			switch (name)
			{
					case Specs.Slash: return 0x01;
					case Specs.Thrust: return 0x02;
					case Specs.Parry: return 0x08;
					case Specs.Sword: return 0x0E;
					case Specs.Hammer: return 0x10;
					case Specs.Axe: return 0x11;
					case Specs.Left_Axe: return 0x12;
					case Specs.Stealth: return 0x13;
					case Specs.Spear: return 0x1A;
					case Specs.Mending: return 0x1D;
					case Specs.Augmentation: return 0x1E;
					case Specs.Crush: return 0x21;
					case Specs.Pacification: return 0x22;
					//				case Specs.Cave_Magic:      return 0x25; ?
					case Specs.Darkness: return 0x26;
					case Specs.Suppression: return 0x27;
					case Specs.Runecarving: return 0x2A;
					case Specs.Shields: return 0x2B;
					case Specs.Flexible: return 0x2E;
					case Specs.Staff: return 0x2F;
					case Specs.Summoning: return 0x30;
					case Specs.Stormcalling: return 0x32;
					case Specs.Beastcraft: return 0x3E;
					case Specs.Polearms: return 0x40;
					case Specs.Two_Handed: return 0x41;
					case Specs.Fire_Magic: return 0x42;
					case Specs.Wind_Magic: return 0x43;
					case Specs.Cold_Magic: return 0x44;
					case Specs.Earth_Magic: return 0x45;
					case Specs.Light: return 0x46;
					case Specs.Matter_Magic: return 0x47;
					case Specs.Body_Magic: return 0x48;
					case Specs.Spirit_Magic: return 0x49;
					case Specs.Mind_Magic: return 0x4A;
					case Specs.Void: return 0x4B;
					case Specs.Mana: return 0x4C;
					case Specs.Dual_Wield: return 0x4D;
					case Specs.CompositeBow: return 0x4E;
					case Specs.Battlesongs: return 0x52;
					case Specs.Enhancement: return 0x53;
					case Specs.Enchantments: return 0x54;
					case Specs.Rejuvenation: return 0x58;
					case Specs.Smite: return 0x59;
					case Specs.Longbow: return 0x5A;
					case Specs.Crossbow: return 0x5B;
					case Specs.Chants: return 0x61;
					case Specs.Instruments: return 0x62;
					case Specs.Blades: return 0x65;
					case Specs.Blunt: return 0x66;
					case Specs.Piercing: return 0x67;
					case Specs.Large_Weapons: return 0x68;
					case Specs.Mentalism: return 0x69;
					case Specs.Regrowth: return 0x6A;
					case Specs.Nurture: return 0x6B;
					case Specs.Nature: return 0x6C;
					case Specs.Music: return 0x6D;
					case Specs.Celtic_Dual: return 0x6E;
					case Specs.Celtic_Spear: return 0x70;
					case Specs.RecurveBow: return 0x71;
					case Specs.Valor: return 0x72;
					case Specs.Pathfinding: return 0x74;
					case Specs.Envenom: return 0x75;
					case Specs.Critical_Strike: return 0x76;
					case Specs.Deathsight: return 0x78;
					case Specs.Painworking: return 0x79;
					case Specs.Death_Servant: return 0x7A;
					case Specs.Soulrending: return 0x7B;
					case Specs.HandToHand: return 0x7C;
					case Specs.Scythe: return 0x7D;
					//				case Specs.Bone_Army:       return 0x7E; ?
					case Specs.Arboreal_Path: return 0x7F;
					case Specs.Creeping_Path: return 0x81;
					case Specs.Verdant_Path: return 0x82;
					case Specs.OdinsWill: return 0x85;
					case Specs.SpectralForce: return 0x86; // Spectral Guard ?
					case Specs.PhantasmalWail: return 0x87;
					case Specs.EtherealShriek: return 0x88;
					case Specs.ShadowMastery: return 0x89;
					case Specs.VampiiricEmbrace: return 0x8A;
					case Specs.Dementia: return 0x8B;
					case Specs.Witchcraft: return 0x8C;
					case Specs.Cursing: return 0x8D;
					case Specs.Hexing: return 0x8E;
					case Specs.Fist_Wraps: return 0x93;
					case Specs.Mauler_Staff: return 0x94;
					case Specs.SpectralGuard: return 0x95;
					case Specs.Archery : return 0x9B;
					default: return 0;
			}
		}
		
		// webdisplay enums: they are processed via /webdisplay command
		public enum eWebDisplay: byte
		{
			all 		= 0x00,
			position 	= 0x01,
			template	= 0x02,
			equipment	= 0x04,
			craft		= 0x08,			
		}
		
		#region AllowedClassesRaces
		/// <summary>
		/// All possible player races
		/// </summary>
		public static readonly Dictionary<eRace, Dictionary<eStat, int>> STARTING_STATS_DICT = new Dictionary<eRace, Dictionary<eStat, int>>()
		{ 
			{ eRace.Unknown, new Dictionary<eStat, int>()			{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Briton, new Dictionary<eStat, int>()			{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Troll, new Dictionary<eStat, int>()				{{eStat.STR, 100}, {eStat.CON, 70}, {eStat.DEX, 35}, {eStat.QUI, 35}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Celt, new Dictionary<eStat, int>()				{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
		};
		/// <summary>
		/// All possible player starting classes
		/// </summary>
		public static readonly Dictionary<eRealm, List<eCharacterClass>> STARTING_CLASSES_DICT = new Dictionary<eRealm, List<eCharacterClass>>()
		{
			// pre 1.93
			{eRealm.Albion, new List<eCharacterClass>() {
				eCharacterClass.Fighter, 
				eCharacterClass.Acolyte, 
				eCharacterClass.Mage, 
				// post 1.93
				eCharacterClass.Armsman,
				eCharacterClass.Cleric,
				eCharacterClass.Wizard,
				eCharacterClass.Fighter,
				eCharacterClass.Acolyte,
				eCharacterClass.Mage,
			}},
			{eRealm.Midgard, new List<eCharacterClass>() {
				eCharacterClass.Viking, 
				eCharacterClass.Mystic, 
				eCharacterClass.Seer,
				// post 1.93
				eCharacterClass.Warrior, 		// Warrior = 22,
				eCharacterClass.Healer, 		// Healer = 26,
				eCharacterClass.Runemaster, 	// Runemaster = 29,
				eCharacterClass.Viking, 		// Viking = 35,
				eCharacterClass.Mystic, 		// Mystic = 36,
				eCharacterClass.Seer, 			// Seer = 37,
			}},
			{eRealm.Hibernia, new List<eCharacterClass>() {
				eCharacterClass.Guardian, 
				eCharacterClass.Naturalist, 
				eCharacterClass.Magician, 
				// post 1.93
				eCharacterClass.Eldritch, 		// Eldritch = 40,
				eCharacterClass.Hero, 		    // Hero = 44,
				eCharacterClass.Druid, 	    // Druid = 47,
				eCharacterClass.Magician, 		// Magician = 51,
				eCharacterClass.Guardian, 		// Guardian = 52,
				eCharacterClass.Naturalist, 	// Naturalist = 53,
			}},
		};

		/// <summary>
		/// Race to Gender Constraints
		/// </summary>
		public static readonly Dictionary<eRace, eGender> RACE_GENDER_CONSTRAINTS_DICT = new Dictionary<eRace, eGender>()
		{
		};
		
		/// <summary>
		/// Class to Gender Constraints
		/// </summary>
		public static readonly Dictionary<eCharacterClass, eGender> CLASS_GENDER_CONSTRAINTS_DICT = new Dictionary<eCharacterClass, eGender>()
		{
		};
		
		/// <summary>
		/// Holds all realm rank names
		/// sirru mod 20.11.06
		/// </summary>
		public static string[, ,] REALM_RANK_NAMES = new string[,,]
		{
			// Albion
			{
				// Male
				{
					"Guardian",
					"Warder",
					"Myrmidon",
					"Gryphon Knight",
					"Eagle Knight",
					"Phoenix Knight",
					"Alerion Knight",
					"Unicorn Knight",
					"Lion Knight",
					"Dragon Knight",
					"Lord",
					"Baronet",
					"Baron",
					"Arch Duke"
				}
				,
				// Female
				{
					"Guardian",
					"Warder",
					"Myrmidon",
					"Gryphon Knight",
					"Eagle Knight",
					"Phoenix Knight",
					"Alerion Knight",
					"Unicorn Knight",
					"Lion Knight",
					"Dragon Knight",
					"Lady",
					"Baronetess",
					"Baroness",
					"Arch Duchess",
				}
			}
			,
			// Midgard
			{
				// Male
				{
					"Skiltvakten",
					"Isen Vakten",
					"Flammen Vakten",
					"Elding Vakten",
					"Stormur Vakten",
					"Isen Herra",
					"Flammen Herra",
					"Elding Herra",
					"Stormur Herra",
					"Einherjar",
					"Herra",
					"Hersir",
					"Vicomte",
					"Stor Jarl"
				}
				,
				// Female
				{
					"Skiltvakten",
					"Isen Vakten",
					"Flammen Vakten",
					"Elding Vakten",
					"Stormur Vakten",
					"Isen Fru",
					"Flammen Fru",
					"Elding Fru",
					"Stormur Fru",
					"Einherjar",
					"Fru",
					"Baronsfru",
					"Vicomtessa",
					"Stor Hurfru",
				}
			}
			,
			// Hibernia
			{
				// Male
				{
					"Savant",
					"Cosantoir",
					"Brehon",
					"Grove Protector",
					"Raven Ardent",
					"Silver Hand",
					"Thunderer",
					"Gilded Spear",
					"Tiarna",
					"Emerald Ridere",
					"Barun",
					"Ard Tiarna",
					"Ciann Cath",
					"Ard Diuc"
				}
				,
				// Female
				{
					"Savant",
					"Cosantoir",
					"Brehon",
					"Grove Protector",
					"Raven Ardent",
					"Silver Hand",
					"Thunderer",
					"Gilded Spear",
					"Bantiarna",
					"Emerald Ridere",
					"Banbharun",
					"Ard Bantiarna",
					"Ciann Cath",
					"Ard Bandiuc"
				}
			}
		};
		
		/// <summary>
		/// Translate Given Race/Gender Combo in Client Language
		/// </summary>
		/// <param name="client"></param>
		/// <param name="race"></param>
		/// <param name="gender"></param>
		/// <returns></returns>
		public static string RaceToTranslatedName(this GameClient client, int race, int gender)
		{
			eRace r = (eRace)race;
			string translationID = string.Format("GamePlayer.PlayerRace.{0}", r.ToString("F")); //Returns 'Unknown'

			if (r != 0)
			{
				switch ((eGender)gender)
				{
					case eGender.Female:
						translationID = string.Format("GamePlayer.PlayerRace.Female.{0}", r.ToString("F"));
						break;
					default:
						translationID = string.Format("GamePlayer.PlayerRace.Male.{0}", r.ToString("F"));
						break;
				}
			}
			
            return LanguageMgr.GetTranslation(client, translationID);
		}
		
		/// <summary>
		/// Translate Given Race/Gender Combo in Player Language
		/// </summary>
		/// <param name="player"></param>
		/// <param name="race"></param>
		/// <param name="gender"></param>
		/// <returns></returns>
		public static string RaceToTranslatedName(this GamePlayer player, int race, eGender gender)
		{
			if (player.Network != null)
				return player.Network.RaceToTranslatedName(race, (int)gender);
			
			return string.Format("!{0} - {1}!", ((eRace)race).ToString("F"), gender.ToString("F"));
		}
		#endregion
		
	}
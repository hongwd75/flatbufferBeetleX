﻿using Game.Logic.Inventory;

namespace Game.Logic.Skills;

public abstract class Abilities
{
	public const string Sprint = "Sprint";
	public const string Quickcast = "QuickCast";

	public const string AlbArmor = "AlbArmor";
	public const string HibArmor = "HibArmor";
	public const string MidArmor = "MidArmor";
	
	public const string Shield = "Shield";

	public const string Weapon_Staves = "Weaponry: Staves";
	public const string Weapon_Archery = "Weaponry: Archery";
	public const string Weapon_Slashing = "Weaponry: Slashing";
	public const string Weapon_Crushing = "Weaponry: Crushing";
	public const string Weapon_Thrusting = "Weaponry: Thrusting";
	public const string Weapon_Polearms = "Weaponry: Polearms";
	public const string Weapon_TwoHanded = "Weaponry: Two Handed";
	public const string Weapon_Flexible = "Weaponry: Flexible";

	public const string Weapon_Crossbow = "Weaponry: Crossbow";
	public const string Weapon_CompositeBows = "Weaponry: Composite Bows";
	public const string Weapon_RecurvedBows = "Weaponry: Recurved Bows";
	public const string Weapon_Shortbows  = "Weaponry: Shortbows";
	public const string Weapon_Longbows  = "Weaponry: Longbows";

	public const string Weapon_Axes = "Weaponry: Axes";
	public const string Weapon_LeftAxes = "Weaponry: Left Axes";
	public const string Weapon_Hammers = "Weaponry: Hammers";
	public const string Weapon_Swords = "Weaponry: Swords";
	public const string Weapon_HandToHand = "Weaponry: Hand to Hand";
	public const string Weapon_Spears = "Weaponry: Spears";
	public const string Weapon_Thrown = "Weaponry: Thrown";
	public const string Weapon_Blades = "Weaponry: Blades";
	public const string Weapon_Blunt = "Weaponry: Blunt";
	public const string Weapon_Piercing = "Weaponry: Piercing";
	public const string Weapon_LargeWeapons = "Weaponry: Large Weapons";
	public const string Weapon_CelticSpear = "Weaponry: Celtic Spears";
	public const string Weapon_Scythe = "Weaponry: Scythe";
	public const string Weapon_Instruments = "Weaponry: Instruments";
	public const string Weapon_MaulerStaff = "Weaponry: Mauler Staff";
	public const string Weapon_FistWraps = "Weaponry: Fist Wraps";
	public const string Advanced_Evade = "Advanced Evade";
	public const string Evade = "Evade";
	public const string Berserk = "Berserk";
	public const string Intercept = "Intercept";
	public const string ChargeAbility = "Charge";
	public const string Flurry = "Flurry";
	public const string Protect = "Protect";
	public const string Critical_Shot = "Critical Shot";
	public const string Camouflage = "Camouflage";
	public const string DirtyTricks = "Dirty Tricks";
	public const string Triple_Wield = "Triple Wield";
	public const string Distraction = "Distraction";
	public const string DetectHidden = "Detect Hidden";
	public const string SafeFall = "Safe Fall";
	public const string Climbing = "Climb Walls";
	public const string ClimbSpikes = "Climbing Spikes";
	public const string DangerSense = "Danger Sense";
	public const string Engage = "Engage";
	public const string Envenom = "Envenom";
	public const string Guard = "Guard";
	public const string PenetratingArrow = "Penetrating Arrow";
	public const string PreventFlight = "Prevent Flight";
	public const string RapidFire = "Rapid Fire";
	public const string Stag = "Stag";
	public const string Stoicism = "Stoicism";
	public const string SureShot = "Sure Shot";
	public const string Tireless = "Tireless";
	public const string VampiirConstitution = "Vampiir Constitution";
	public const string VampiirDexterity = "Vampiir Dexterity";
	public const string VampiirQuickness = "Vampiir Quickness";
	public const string VampiirStrength = "Vampiir Strength";
	public const string VampiirBolt = "Vampiir Bolt";
	public const string Volley = "Volley";
	public const string BloodRage = "Blood Rage";
	public const string SubtleKills = "Subtle Kills";
	public const string HeightenedAwareness = "Heightened Awareness";
	public const string ScarsOfBattle = "Scars of Battle";
	public const string MemoriesOfWar = "Memories of War";
	public const string Snapshot = "Snapshot";
	public const string Rampage = "Rampage";
	public const string MetalGuard = "Metal Guard";
	public const string Fury = "Fury";
	public const string Bodyguard = "Bodyguard";
	public const string BolsteringRoar = "Bolstering Roar";
	public const string TauntingShout = "Taunting Shout";
    public const string Remedy = "Remedy";
	public const string DefensiveCombatPowerRegeneration = "Defensive Combat Power Regeneration";
	public const string CCImmunity = "CCImmunity";
	public const string DamageImmunity = "DamageImmunity";
	public const string RootImmunity = "RootImmunity";
	public const string MezzImmunity = "MezzImmunity";
	public const string StunImmunity = "StunImmunity";

	public static eObjectType AbilityToWeapon( string abilityKeyName )
	{
		eObjectType type = eObjectType.GenericItem;

		switch ( abilityKeyName )
		{
			case Abilities.Shield:
				type = eObjectType.Shield; break;
			case Abilities.Weapon_Axes:
				type = eObjectType.Axe; break;
			case Abilities.Weapon_Blades:
				type = eObjectType.Blades; break;
			case Abilities.Weapon_Blunt:
				type = eObjectType.Blunt; break;
			case Abilities.Weapon_CelticSpear:
				type = eObjectType.CelticSpear; break;
			case Abilities.Weapon_CompositeBows:
				type = eObjectType.CompositeBow; break;
			case Abilities.Weapon_Crossbow:
				type = eObjectType.Crossbow; break;
			case Abilities.Weapon_Crushing:
				type = eObjectType.CrushingWeapon; break;
			case Abilities.Weapon_FistWraps:
				type = eObjectType.FistWraps; break;
			case Abilities.Weapon_Flexible:
				type = eObjectType.Flexible; break;
			case Abilities.Weapon_Hammers:
				type = eObjectType.Hammer; break;
			case Abilities.Weapon_HandToHand:
				type = eObjectType.HandToHand; break;
			case Abilities.Weapon_Instruments:
				type = eObjectType.Instrument; break;
			case Abilities.Weapon_LargeWeapons:
				type = eObjectType.LargeWeapons; break;
			case Abilities.Weapon_LeftAxes:
				type = eObjectType.LeftAxe; break;
			case Abilities.Weapon_Longbows:
				type = eObjectType.Longbow; break;
			case Abilities.Weapon_MaulerStaff:
				type = eObjectType.MaulerStaff; break;
			case Abilities.Weapon_Piercing:
				type = eObjectType.Piercing; break;
			case Abilities.Weapon_Polearms:
				type = eObjectType.PolearmWeapon; break;
			case Abilities.Weapon_RecurvedBows:
				type = eObjectType.RecurvedBow; break;
			case Abilities.Weapon_Scythe:
				type = eObjectType.Scythe; break;
			case Abilities.Weapon_Shortbows:
				type = eObjectType.Fired; break;
			case Abilities.Weapon_Slashing:
				type = eObjectType.SlashingWeapon; break;
			case Abilities.Weapon_Spears:
				type = eObjectType.Spear; break;
			case Abilities.Weapon_Staves:
				type = eObjectType.Staff; break;
			case Abilities.Weapon_Swords:
				type = eObjectType.Sword; break;
			case Abilities.Weapon_Thrown:
				type = eObjectType.Thrown; break;
			case Abilities.Weapon_Thrusting:
				type = eObjectType.ThrustWeapon; break;
			case Abilities.Weapon_TwoHanded:
				type = eObjectType.TwoHandedWeapon; break;
		}

		return type;
	}
}

/// <summary>
/// strong name constants for built in specs
/// </summary>
public abstract class Specs
{
	public const string Slash = "Slash";
	public const string Crush = "Crush";
	public const string Thrust = "Thrust";
	public const string Piercing = "Piercing";
	public const string Staff = "Staff";
	public const string Flexible = "Flexible";
	public const string ShortBow = "Shortbow";
	public const string Longbow = "Longbows";
	public const string Crossbow = "Crossbows";
	public const string CompositeBow = "Composite Bow";
	public const string RecurveBow = "Recurve Bow";
	public const string Sword = "Sword";
	public const string Axe = "Axe";
	public const string Hammer = "Hammer";
	public const string Shields = "Shields";
	public const string Left_Axe = "Left Axe";
	public const string Two_Handed = "Two Handed";
	public const string Thrown_Weapons = "Thrown Weapons";
	public const string Polearms = "Polearm";
	public const string Blades = "Blades";
	public const string Blunt = "Blunt";
	public const string Critical_Strike = "Critical Strike";
	public const string Dual_Wield = "Dual Wield";
	public const string Spear = "Spear";
	public const string Large_Weapons = "Large Weapons";
	public const string HandToHand = "Hand to Hand";
	public const string Scythe = "Scythe";
	public const string Celtic_Dual = "Celtic Dual";
	public const string Celtic_Spear = "Celtic Spear";
	public const string Stealth = "Stealth";
	public const string Parry = "Parry";
	public const string Envenom = "Envenom";
	public const string Savagery = "Savagery";
	public const string Pathfinding = "Pathfinding";
	public const string Nightshade_Magic = "Nightshade Magic";
	public const string Augmentation  = "Augmentation";
	public const string Battlesongs   = "Battlesongs";
	public const string Beastcraft    = "Beastcraft";
	public const string Body_Magic    = "Body Magic";
	public const string Chants        = "Chants";
	public const string Cold_Magic    = "Cold Magic";
	public const string Darkness      = "Darkness";
	public const string Earth_Magic   = "Earth Magic";
	public const string Enchantments  = "Enchantments";
	public const string Enhancement   = "Enhancement";
	public const string Fire_Magic    = "Fire Magic";
	public const string Instruments   = "Instruments";
	public const string Light         = "Light";
	public const string Mana          = "Mana";
	public const string Matter_Magic  = "Matter Magic";
	public const string Mending       = "Mending";
	public const string Mentalism     = "Mentalism";
	public const string Mind_Magic    = "Mind Magic";
	public const string Music         = "Music";
	public const string Nature        = "Nature";
	public const string Nurture       = "Nurture";
	public const string Pacification  = "Pacification";
	public const string Regrowth      = "Regrowth";
	public const string Rejuvenation  = "Rejuvenation";
	public const string Runecarving   = "Runecarving";
	public const string Smite         = "Smite";
	public const string Spirit_Magic  = "Spirit Magic";
	public const string Stormcalling  = "Stormcalling";
	public const string Subterranean  = "Subterranean";
	public const string Summoning     = "Summoning";
	public const string Suppression   = "Suppression";
	public const string Valor         = "Valor";
	public const string Void          = "Void";
	public const string Wind_Magic    = "Wind Magic";
	public const string Arboreal_Path = "Arboreal Path"; //Forester
	public const string BoneArmy   = "Bone Army"; //Bonedancer
	public const string Creeping_Path = "Creeping Path"; //Animist
	public const string Cursing 	  = "Cursing"; //Warlock
	public const string Death_Servant = "Death Servant"; //Necro
	public const string Deathsight    = "Deathsight"; //Necro
	public const string Dementia      = "Dementia"; //Vampiir
	public const string EtherealShriek = "Ethereal Shriek"; //Bainshee
	public const string Hexing 	  = "Hexing"; //Warlock
	public const string OdinsWill  	  = "Odin's Will"; //Valkyrie
	public const string Painworking   = "Painworking"; //Necro
	public const string PhantasmalWail = "Phantasmal Wail"; //Bainshee
	public const string ShadowMastery = "Shadow Mastery"; //Vampiir
	public const string Soulrending   = "Soulrending"; //Reaver
	public const string SpectralForce = "Spectral Force"; //Bainshee
	public const string SpectralGuard = "Spectral Guard"; //Bainshee
	public const string VampiiricEmbrace = "Vampiiric Embrace"; //Vampiir
	public const string Verdant_Path  = "Verdant Path"; //Animist
	public const string Witchcraft  = "Witchcraft"; //Warlock
	public const string Aura_Manipulation = "Aura Manipulation";
	public const string Magnetism = "Magnetism";
	public const string Power_Strikes = "Power Strikes";
	public const string Mauler_Staff = "Mauler Staff";
	public const string Fist_Wraps = "Fist Wraps";
	public const string Archery = "Archery";
	public const string Beastcraft_NEW = "Beastcraft";
}
using Game.Logic.Inventory;
using Game.Logic.Language;
using Game.Logic.network;
using Game.Logic.PropertyCalc;
using Game.Logic.Skills;
using Game.Logic.Utils;
using Game.Logic.World;
using Logic.database.table;

namespace Game.Logic;

public enum eGameServerType
{
    GST_Normal = 0,
    GST_Casual = 1,
    GST_PvE = 2,
    MAX = 3,
}

public enum eInstrumentType : int
{
	Drum = 1,
	Lute = 2,
	Flute = 3,
	Harp = 4,
}

public enum eWeaponDamageType : byte
{
	Elemental = 0,
	Crush = 1,
	Slash = 2,
	Thrust = 3,

	Body = 10,
	Cold = 11,
	Energy = 12,
	Heat = 13,
	Matter = 14,
	Spirit = 15,
}

public abstract class GlobalSpellsLines
{
    public const string Combat_Styles_Effect = "Combat Style Effects";
    public const string Mundane_Poisons = "Mundane Poisons";
    public const string Reserved_Spells = "Reserved Spells"; // Masterlevels
    public const string SiegeWeapon_Spells = "SiegeWeapon Spells";
    public const string Item_Effects = "Item Effects";
    public const string Potions_Effects = "Potions";
    public const string Mob_Spells = "Mob Spells";
    public const string Character_Abilities = "Character Abilities"; // dirty tricks, flurry ect...
    public const string Item_Spells = "Item Spells";	// Combine scroll etc.
    public const string Champion_Lines_StartWith = "Champion ";
    public const string Realm_Spells = "Realm Spells"; // Resurrection illness, Speed of the realm
}

public abstract class ShieldLevel
{
	public const int Small = 1;
	public const int Medium = 2;
	public const int Large = 3;
}	
	
public static class Constants
{
	public static int USE_AUTOVALUES = -1;
}
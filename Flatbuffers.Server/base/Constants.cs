namespace Game.Logic;

public enum eGameServerType
{
    GST_Normal = 0,
    GST_Casual = 1,
    GST_PvE = 2,
    MAX = 3,
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
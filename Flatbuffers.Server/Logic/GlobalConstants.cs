using Game.Logic.Inventory;

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
}
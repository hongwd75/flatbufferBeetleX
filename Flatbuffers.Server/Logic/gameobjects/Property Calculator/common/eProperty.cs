namespace Game.Logic.PropertyCalc;

public enum eBuffBonusType : byte
{
    Debuff = 0,
    SpecDebuff,
    BaseBuff,
    SpecBuff,
    ItemBonus,
    AbilityBonus,
    BuffBonus,              //BuffBonusCategory4
    MaxBonusType
}

public enum eProperty : byte
{
    Undefined = 0,
    // Note, these are set in the ItemDB now.  Changing
    //any order will screw things up.
    // char stats
    #region Stats
    Stat_First = 1,
    Strength = 1,
    Dexterity = 2,
    Constitution = 3,
    Quickness = 4,
    Intelligence = 5,
    Piety = 6,
    Empathy = 7,
    Charisma = 8,
    Stat_Last = 8,
    MaxMana = 9,
    MaxHealth = 10,
    #endregion

    #region Resists
    // resists
    Resist_First = 11,
    Resist_Body = 11,
    Resist_Cold = 12,
    Resist_Crush = 13,
    Resist_Energy = 14,
    Resist_Heat = 15,
    Resist_Matter = 16,
    Resist_Slash = 17,
    Resist_Spirit = 18,
    Resist_Thrust = 19,
    Resist_Last = 19,
    #endregion
    
    #region Cap Bonuses
    //Caps bonuses
    StatCapBonus_First = 20,
    StrCapBonus = 21,
    DexCapBonus = 22,
    ConCapBonus = 23,
    QuiCapBonus = 24,
    IntCapBonus = 25,
    PieCapBonus = 26,
    EmpCapBonus = 27,
    ChaCapBonus = 28,
    AcuCapBonus = 29,
    MaxHealthCapBonus = 30,
    PowerPoolCapBonus = 31,
    StatCapBonus_Last = 32,
    #endregion
    
    #region Resist Cap Increases
    //Resist cap increases
    ResCapBonus_First = 33,
    BodyResCapBonus = 34,
    ColdResCapBonus = 35,
    CrushResCapBonus = 36,
    EnergyResCapBonus = 37,
    HeatResCapBonus = 38,
    MatterResCapBonus = 39,
    SlashResCapBonus = 40,
    SpiritResCapBonus = 41,
    ThrustResCapBonus = 42,
    ResCapBonus_Last = 43,
    #endregion    
    
    MaxSpeed = 44,
    MaxConcentration = 45,
    ArmorFactor = 46,
    ArmorAbsorption = 47,
    HealthRegenerationRate = 48,
    PowerRegenerationRate = 49,
    EnduranceRegenerationRate = 50,
    SpellRange = 51,
    ArcheryRange = 52,
    MeleeSpeed = 53,
    Acuity = 54,
    EvadeChance = 55,
    BlockChance = 56,
    ParryChance = 57,
    FatigueConsumption = 58,
    MeleeDamage = 59,
    RangedDamage = 60,
    FumbleChance = 61,
    MesmerizeDurationReduction = 62,
    StunDurationReduction = 63,
    SpeedDecreaseDurationReduction = 64,
    BladeturnReinforcement = 65,
    DefensiveBonus = 66,
    SpellFumbleChance = 67,
    NegativeReduction = 68,
    PieceAblative = 69,
    ReactionaryStyleDamage = 70,
    SpellPowerCost = 71,
    StyleCostReduction = 72,
    ToHitBonus = 73,    
    MagicAbsorption = 74,
    Resist_Natural = 75,
    WeaponSkill = 76,
    CriticalMeleeHitChance = 77,
    CriticalSpellHitChance = 78,
    CriticalHealHitChance = 79,
    AllSkills = 80,
    WaterSpeed = 81,
    SpellLevel = 82,
    MissHit = 83,
    DPS = 84,
    #region TOA
    //TOA
    ToABonus_First = 85,
    BuffEffectiveness = 86,
    CastingSpeed = 87,
    DebuffEffectivness = 88,
    HealingEffectiveness = 89,
    PowerPool = 90,
    ResistPierce = 91,
    SpellDamage = 92,
    SpellDuration = 93,
    StyleDamage = 94,
    ToABonus_Last = 95,
    #endregion    
    StyleAbsorb = 96,
    LivingEffectiveLevel = 97,
    
    #region === SKILLS ===
    Skill_First = 100,
    Skill_Stealth = 100,
    Skill_Last = 200,
    #endregion
    
    MaxProperty = 255
}

/// <summary>
/// The type of stat
/// </summary>
public enum eStat : byte
{
    UNDEFINED = 0,
    _First = eProperty.Stat_First,
    STR = eProperty.Strength,
    DEX = eProperty.Dexterity,
    CON = eProperty.Constitution,
    QUI = eProperty.Quickness,
    INT = eProperty.Intelligence,
    PIE = eProperty.Piety,
    EMP = eProperty.Empathy,
    CHR = eProperty.Charisma,
    _Last = eProperty.Stat_Last,
}

/// <summary>
/// resists
/// </summary>
public enum eResist : byte
{
    Natural = eProperty.Resist_Natural,
    Crush = eProperty.Resist_Crush,
    Slash = eProperty.Resist_Slash,
    Thrust = eProperty.Resist_Thrust,
    Body = eProperty.Resist_Body,
    Cold = eProperty.Resist_Cold,
    Energy = eProperty.Resist_Energy,
    Heat = eProperty.Resist_Heat,
    Matter = eProperty.Resist_Matter,
    Spirit = eProperty.Resist_Spirit
}

/// <summary>
///  ChageType hp / mana / endu
/// </summary>
public enum eChargeChangeType : byte
{
    /// <summary>
    /// The health was changed by something unknown
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Regeneration changed the health
    /// </summary>
    Regenerate = 1,
    /// <summary>
    /// A spell changed the health
    /// </summary>
    Spell = 2,
    /// <summary>
    /// A potion changed the health
    /// </summary>
    Potion = 3
}
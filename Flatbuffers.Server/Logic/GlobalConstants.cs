namespace Game.Logic;

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

public enum eTranslationIdentifier : byte
{
    eArea = 0,
    eDoor = 1,
    eItem = 2,
    eNPC = 3,
    eObject = 4,
    eSystem = 5,
    eZone = 6
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
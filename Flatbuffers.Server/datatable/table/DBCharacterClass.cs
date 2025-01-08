using Logic.database.attribute;

namespace Logic.database.table;

[DataTable(TableName = "CharacterClass")]
public class DBCharacterClass : DataObject
{
    private byte id;
    private byte baseClassID;
    private byte specPointMultiplier;
    private short baseHP;
    private short baseWeaponSkill;
    private string autoTrainableSkills = "";
    private string eligibleRaces = "";
    private byte classType;
    private string name;
    private string femaleName = "";
    private string professionTranslationID;
    private byte primaryStat;
    private byte secondaryStat;
    private byte tertiaryStat;
    private byte manaStat;
    private bool canUseLeftHandedWeapon;
    private byte maxPulsingSpells;

    [PrimaryKey]
    public byte ID
    {
        get => id;
        set
        {
            Dirty = true;
            id = value;
        }
    }

    [DataElement]
    public byte BaseClassID
    {
        get => baseClassID;
        set
        {
            Dirty = true;
            baseClassID = value;
        }
    }

    [DataElement]
    public byte ClassType
    {
        get => classType;
        set
        {
            Dirty = true;
            classType = value;
        }
    }

    [DataElement]
    public string Name
    {
        get => name;
        set
        {
            Dirty = true;
            name = value;
        }
    }

    [DataElement]
    public string FemaleName
    {
        get => femaleName;
        set
        {
            Dirty = true;
            femaleName = value;
        }
    }

    [DataElement]
    public byte SpecPointMultiplier
    {
        get => specPointMultiplier;
        set
        {
            Dirty = true;
            specPointMultiplier = value;
        }
    }

    [DataElement]
    public string AutoTrainSkills
    {
        get => autoTrainableSkills;
        set
        {
            Dirty = true;
            autoTrainableSkills = value;
        }
    }


    [DataElement]
    public byte PrimaryStat
    {
        get => primaryStat;
        set
        {
            Dirty = true;
            primaryStat = value;
        }
    }

    [DataElement]
    public byte SecondaryStat
    {
        get => secondaryStat;
        set
        {
            Dirty = true;
            secondaryStat = value;
        }
    }

    [DataElement]
    public byte TertiaryStat
    {
        get => tertiaryStat;
        set
        {
            Dirty = true;
            tertiaryStat = value;
        }
    }

    [DataElement]
    public byte ManaStat
    {
        get => manaStat;
        set
        {
            Dirty = true;
            manaStat = value;
        }
    }

    [DataElement]
    public short BaseHP
    {
        get => baseHP;
        set
        {
            Dirty = true;
            baseHP = value;
        }
    }

    [DataElement]
    public short BaseWeaponSkill
    {
        get => baseWeaponSkill;
        set
        {
            Dirty = true;
            baseWeaponSkill = value;
        }
    }

    [DataElement]
    public string EligibleRaces
    {
        get => eligibleRaces;
        set
        {
            Dirty = true;
            eligibleRaces = value;
        }
    }

    [DataElement]
    public bool CanUseLeftHandedWeapon
    {
        get => canUseLeftHandedWeapon;
        set
        {
            Dirty = true;
            canUseLeftHandedWeapon = value;
        }
    }

    [DataElement]
    public string ProfessionTranslationID
    {
        get => professionTranslationID;
        set
        {
            Dirty = true;
            professionTranslationID = value;
        }
    }

    //No DataElement on purpose
    public byte MaxPulsingSpells
    {
        get => maxPulsingSpells;
        set
        {
            Dirty = true;
            maxPulsingSpells = value;
        }
    }
}
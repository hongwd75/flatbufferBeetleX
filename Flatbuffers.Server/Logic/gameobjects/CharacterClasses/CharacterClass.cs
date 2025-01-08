using Game.Logic.Language;
using Game.Logic.PropertyCalc;
using Game.Logic.Utils;
using Logic.database.table;

namespace Game.Logic.CharacterClasses;

public class CharacterClass
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private static Dictionary<int, CharacterClass> allClasses = new Dictionary<int, CharacterClass>();

    private string name;
    private string femaleName;
    private IEnumerable<PlayerRace> eligibleRaces;
    private int baseClassID;

    public int ID { get; private set; }
    public eClassType ClassType { get; private set; }
    public int SpecPointsMultiplier { get; private set; }
    public IEnumerable<string> AutoTrainSkills { get; private set; }
    public eStat PrimaryStat { get; private set; }
    public eStat SecondaryStat { get; private set; }
    public eStat TertiaryStat { get; private set; }
    public eStat ManaStat { get; private set; }
    public bool CanUseLefthandedWeapon { get; private set; }
    public int BaseHP { get; private set; }
    public int WeaponSkillBase { get; private set; }
    public string ProfessionTranslationID { get; private set; } = "";
    public ushort MaxPulsingSpells { get; private set; }

    public string BaseName => GetClass(baseClassID).name;
    public List<PlayerRace> EligibleRaces => eligibleRaces.ToList();
    public int AdjustedSpecPointsMultiplier => SpecPointsMultiplier;

    public IList<string> GetAutotrainableSkills()
        => AutoTrainSkills.ToList();

    public bool HasAdvancedFromBaseClass()
        => ID != baseClassID;

    private CharacterClass() { }

    public static CharacterClass Create(DBCharacterClass dbCharClass)
    {
        var charClass = new CharacterClass();
        charClass.ID = dbCharClass.ID;
        charClass.baseClassID = dbCharClass.BaseClassID;
        charClass.name = dbCharClass.Name;
        charClass.femaleName = dbCharClass.FemaleName;
        charClass.ClassType = (eClassType)dbCharClass.ClassType;
        charClass.SpecPointsMultiplier = dbCharClass.SpecPointMultiplier;
        charClass.BaseHP = dbCharClass.BaseHP;
        charClass.WeaponSkillBase = dbCharClass.BaseWeaponSkill;
        charClass.ManaStat = (eStat)dbCharClass.ManaStat;
        charClass.PrimaryStat = (eStat)dbCharClass.PrimaryStat;
        charClass.SecondaryStat = (eStat)dbCharClass.SecondaryStat;
        charClass.TertiaryStat = (eStat)dbCharClass.TertiaryStat;
        charClass.CanUseLefthandedWeapon = dbCharClass.CanUseLeftHandedWeapon;
        charClass.ProfessionTranslationID = dbCharClass.ProfessionTranslationID;

        charClass.AutoTrainSkills = dbCharClass.AutoTrainSkills
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

        var eligibleRaces = new List<PlayerRace>();
        var raceIDs = dbCharClass.EligibleRaces
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Convert.ToInt32(s));
        foreach (var raceID in raceIDs)
        {
            var race = PlayerRace.GetRace(raceID);
            if (race.Equals(PlayerRace.Unknown)) log.Error($"CharacterClass with ID {charClass.ID} contains invalid EligibleRace {raceID}.");
            else eligibleRaces.Add(race);
        }
        charClass.eligibleRaces = eligibleRaces;

        charClass.MaxPulsingSpells = dbCharClass.MaxPulsingSpells == 0 ? (byte)2 : dbCharClass.MaxPulsingSpells;

        return charClass;
    }

    public string GetSalutation(eGender gender)
    {
        var femaleName = this.femaleName;
        var useFemaleName = (gender == eGender.Female && !string.IsNullOrEmpty(femaleName));
        if (useFemaleName) return femaleName;
        else return name;
    }

    public string GetProfessionTitle(GamePlayer player)
        => LanguageMgr.TryTranslateOrDefault(player, ProfessionTranslationID, ProfessionTranslationID);

    public string GetTitle(GamePlayer player, int level)
    {
        if (!HasAdvancedFromBaseClass()) level = 0;

        // Clamp level in 5 by 5 steps - 50 is the max available translation for now
        int clamplevel = Math.Min(50, (level / 5) * 5);

        string none = LanguageMgr.TryTranslateOrDefault(player, "!None!", "PlayerClass.GetTitle.none");

        if (clamplevel > 0)
            return LanguageMgr.TryTranslateOrDefault(player, string.Format("!{0}!", name), string.Format("PlayerClass.{0}.GetTitle.{1}", name, clamplevel));

        return none;
    }

    public static CharacterClass GetClass(int classID)
    {
        allClasses.TryGetValue(classID, out var characterClass);
        if (characterClass == null) return Unknown;
        return characterClass;
    }

    public CharacterClass GetBaseClass()
        => GetClass(baseClassID);

    public static void AddOrReplace(CharacterClass charClass)
    {
        allClasses[charClass.ID] = charClass;
    }

    public static CharacterClass Unknown
        => new CharacterClass()
        {
            ID = 0,
            name = "Unknown Class",
            baseClassID = 0,
            femaleName = "",
            ProfessionTranslationID = "",
            SpecPointsMultiplier = 10,
            AutoTrainSkills = new string[] { },
            PrimaryStat = eStat.UNDEFINED,
            SecondaryStat = eStat.UNDEFINED,
            TertiaryStat = eStat.UNDEFINED,
            ManaStat = eStat.UNDEFINED,
            eligibleRaces = PlayerRace.AllRaces,
            CanUseLefthandedWeapon = false,
            BaseHP = 600,
            WeaponSkillBase = 400,
            ClassType = eClassType.ListCaster,
        };

    public override bool Equals(object obj)
    {
        if (obj is CharacterClass characterClass)
        {
            return characterClass.ID == ID;
        }
        return false;
    }

    public override int GetHashCode()
        => ID;

    #region CharacterClass(Base) creation shortcuts
    public static CharacterClass None => CharacterClass.GetClass((int)eCharacterClass.Unknown);
    //alb
    public static CharacterClass Fighter => CharacterClass.GetClass((int)eCharacterClass.Fighter);
    public static CharacterClass Acolyte => CharacterClass.GetClass((int)eCharacterClass.Acolyte);
    public static CharacterClass Mage => CharacterClass.GetClass((int)eCharacterClass.Mage);
    public static CharacterClass Armsman => CharacterClass.GetClass((int)eCharacterClass.Armsman);
    public static CharacterClass Cleric => CharacterClass.GetClass((int)eCharacterClass.Cleric);
    public static CharacterClass Wizard => CharacterClass.GetClass((int)eCharacterClass.Wizard);
    //mid
    public static CharacterClass Viking => CharacterClass.GetClass((int)eCharacterClass.Viking);
    public static CharacterClass Seer => CharacterClass.GetClass((int)eCharacterClass.Seer);
    public static CharacterClass Mystic => CharacterClass.GetClass((int)eCharacterClass.Mystic);
    public static CharacterClass Healer => CharacterClass.GetClass((int)eCharacterClass.Healer);
    public static CharacterClass Runemaster => CharacterClass.GetClass((int)eCharacterClass.Runemaster);
    public static CharacterClass Warrior => CharacterClass.GetClass((int)eCharacterClass.Warrior);
    //hib
    public static CharacterClass Guardian => CharacterClass.GetClass((int)eCharacterClass.Guardian);
    public static CharacterClass Naturalist => CharacterClass.GetClass((int)eCharacterClass.Naturalist);
    public static CharacterClass Magician => CharacterClass.GetClass((int)eCharacterClass.Magician);
    public static CharacterClass Druid => CharacterClass.GetClass((int)eCharacterClass.Druid);
    public static CharacterClass Eldritch => CharacterClass.GetClass((int)eCharacterClass.Eldritch);
    public static CharacterClass Hero => CharacterClass.GetClass((int)eCharacterClass.Hero);
    #endregion
}

public enum eClassType : int
{
    ListCaster, // access to all spells
    Hybrid, // access to best two spells
    PureTank, // no spells
}
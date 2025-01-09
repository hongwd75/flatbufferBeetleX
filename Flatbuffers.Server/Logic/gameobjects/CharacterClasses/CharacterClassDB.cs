using Game.Logic.datatable;
using Game.Logic.PropertyCalc;
using Logic.database.table;

namespace Game.Logic.CharacterClasses;

public class CharacterClassDB
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    public static void Load()
    {
        var dbClasses = GameDB<DBCharacterClass>.SelectAllObjects().ToDictionary(c => c.ID, c => c);
        foreach (var classID in defaultClasses.Keys.ToList().Union(dbClasses.Keys))
        {
            CharacterClass charClass;
            if (dbClasses.TryGetValue(classID, out var databaseEntry))
            {
                try
                {
                    charClass = CharacterClass.Create(databaseEntry);
                }
                catch (Exception e)
                {
                    log.Error($"CharacterClass with ID {classID} could not be loaded from database. Load default instead.:\n{e}");
                    charClass = CharacterClass.Create(defaultClasses[classID]);
                }
            }
            else
            {
                var dbClass = defaultClasses[classID];
                dbClass.AllowAdd = true;
                GameServer.Database.AddObject(dbClass);
                charClass = CharacterClass.Create(dbClass);
            }

            CharacterClass.AddOrReplace(charClass);
        }
    }

    private static Dictionary<byte, DBCharacterClass> defaultClasses = new Dictionary<byte, DBCharacterClass>()
    {
        {
            (byte)eCharacterClass.Fighter,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Fighter,
                Name = "Fighter",
                BaseClassID = (byte)eCharacterClass.Fighter,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Celt.ID)}",
                BaseHP = 880,
                BaseWeaponSkill = 440,
                ClassType = (byte)eClassType.PureTank,
            }
        },
        {
            (byte)eCharacterClass.Acolyte,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Acolyte,
                Name = "Acolyte",
                BaseClassID = (byte)eCharacterClass.Acolyte,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                ManaStat = (int)eStat.PIE,
                EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Celt.ID)}",
                BaseHP = 720,
                BaseWeaponSkill = 320,
                ClassType = (byte)eClassType.Hybrid,
            }
        },
        {
            (byte)eCharacterClass.Mage,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Mage,
                Name = "Mage",
                BaseClassID = (byte)eCharacterClass.Mage,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                ManaStat = (int)eStat.INT,
                EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Celt.ID)}",
                BaseHP = 560,
                BaseWeaponSkill = 280,
                ClassType = (byte)eClassType.ListCaster,
            }
        },
        {
            (byte)eCharacterClass.Cleric,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Cleric,
                Name = "Cleric",
                BaseClassID = (byte)eCharacterClass.Acolyte,
                ProfessionTranslationID = "PlayerClass.Profession.ChurchofAlbion",
                SpecPointMultiplier = 10,
                PrimaryStat = (int)eStat.PIE,
                SecondaryStat = (int)eStat.CON,
                TertiaryStat = (int)eStat.STR,
                ManaStat = (int)eStat.PIE,
                EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Celt.ID)}",
                BaseHP = 720,
                BaseWeaponSkill = 320,
                ClassType = (byte)eClassType.Hybrid,
            }
        },
        {
            (byte)eCharacterClass.Wizard,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Wizard,
                Name = "Wizard",
                BaseClassID = (byte)eCharacterClass.Mage,
                ProfessionTranslationID = "PlayerClass.Profession.Academy",
                SpecPointMultiplier = 10,
                PrimaryStat = (int)eStat.INT,
                SecondaryStat = (int)eStat.DEX,
                TertiaryStat = (int)eStat.QUI,
                ManaStat = (int)eStat.INT,
                EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Celt.ID)}",
                BaseHP = 560,
                BaseWeaponSkill = 240,
                ClassType = (byte)eClassType.ListCaster,
            }
        },
        {
            (byte)eCharacterClass.Armsman,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Armsman,
                Name = "Armsman",
                BaseClassID = (byte)eCharacterClass.Fighter,
                FemaleName = "Armswoman",
                ProfessionTranslationID = "PlayerClass.Profession.DefendersofAlbion",
                SpecPointMultiplier = 20,
                PrimaryStat = (int)eStat.STR,
                SecondaryStat = (int)eStat.CON,
                TertiaryStat = (int)eStat.DEX,
                EligibleRaces = $"{(int)(PlayerRace.Briton.ID)},{(int)(PlayerRace.Celt.ID)}",
                BaseHP = 880,
                BaseWeaponSkill = 440,
                ClassType = (byte)eClassType.PureTank,
            }
        },
        {
            (byte)eCharacterClass.Viking,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Viking,
                Name = "Viking",
                BaseClassID = (byte)eCharacterClass.Viking,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                EligibleRaces = $"{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Briton.ID)}",
                BaseHP = 880,
                BaseWeaponSkill = 440,
                ClassType = (byte)eClassType.PureTank,
            }
        },
        {
            (byte)eCharacterClass.Mystic,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Mystic,
                Name = "Mystic",
                BaseClassID = (byte)eCharacterClass.Mystic,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                ManaStat = (int)eStat.PIE,
                EligibleRaces = $"{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Briton.ID)}",
                BaseHP = 560,
                BaseWeaponSkill = 280,
                ClassType = (byte)eClassType.ListCaster,
            }
        },
        {
            (byte)eCharacterClass.Seer,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Seer,
                Name = "Seer",
                BaseClassID = (byte)eCharacterClass.Seer,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                ManaStat = (int)eStat.PIE,
                EligibleRaces = $"{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Briton.ID)}",
                BaseHP = 720,
                BaseWeaponSkill = 360,
                ClassType = (byte)eClassType.Hybrid,
            }
        },
        {
            (byte)eCharacterClass.Warrior,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Warrior,
                Name = "Warrior",
                BaseClassID = (byte)eCharacterClass.Viking,
                ProfessionTranslationID = "PlayerClass.Profession.HouseofTyr",
                SpecPointMultiplier = 20,
                PrimaryStat = (int)eStat.STR,
                SecondaryStat = (int)eStat.CON,
                TertiaryStat = (int)eStat.DEX,
                EligibleRaces = $"{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Briton.ID)}",
                BaseHP = 880,
                BaseWeaponSkill = 460,
                ClassType = (byte)eClassType.PureTank,
            }
        },
        {
            (byte)eCharacterClass.Healer,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Healer,
                Name = "Healer",
                BaseClassID = (byte)eCharacterClass.Seer,
                ProfessionTranslationID = "PlayerClass.Profession.HouseofEir",
                SpecPointMultiplier = 10,
                PrimaryStat = (int)eStat.PIE,
                SecondaryStat = (int)eStat.CON,
                TertiaryStat = (int)eStat.STR,
                ManaStat = (int)eStat.PIE,
                EligibleRaces = $"{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Briton.ID)}",
                BaseHP = 720,
                BaseWeaponSkill = 360,
                ClassType = (byte)eClassType.Hybrid,
            }
        },
        {
            (byte)eCharacterClass.Runemaster,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Runemaster,
                Name = "Runemaster",
                BaseClassID = (byte)eCharacterClass.Mystic,
                ProfessionTranslationID = "PlayerClass.Profession.HouseofOdin",
                SpecPointMultiplier = 10,
                PrimaryStat = (int)eStat.PIE,
                SecondaryStat = (int)eStat.DEX,
                TertiaryStat = (int)eStat.QUI,
                ManaStat = (int)eStat.PIE,
                EligibleRaces = $"{(int)(PlayerRace.Troll.ID)},{(int)(PlayerRace.Briton.ID)}",
                BaseHP = 560,
                BaseWeaponSkill = 280,
                ClassType = (byte)eClassType.ListCaster,
            }
        },
        {
            (byte)eCharacterClass.Eldritch,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Eldritch,
                Name = "Eldritch",
                BaseClassID = (byte)eCharacterClass.Magician,
                ProfessionTranslationID = "PlayerClass.Profession.PathofFocus",
                SpecPointMultiplier = 10,
                PrimaryStat = (int)eStat.INT,
                SecondaryStat = (int)eStat.DEX,
                TertiaryStat = (int)eStat.QUI,
                ManaStat = (int)eStat.INT,
                EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Troll.ID)}",
                BaseHP = 560,
                BaseWeaponSkill = 280,
                ClassType = (byte)eClassType.ListCaster,
            }
        },
        {
            (byte)eCharacterClass.Hero,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Hero,
                Name = "Hero",
                BaseClassID = (byte)eCharacterClass.Guardian,
                FemaleName = "Heroine",
                ProfessionTranslationID = "PlayerClass.Profession.PathofFocus",
                SpecPointMultiplier = 20,
                PrimaryStat = (int)eStat.STR,
                SecondaryStat = (int)eStat.CON,
                TertiaryStat = (int)eStat.DEX,
                EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Troll.ID)}",
                BaseHP = 880,
                BaseWeaponSkill = 440,
                ClassType = (byte)eClassType.PureTank,
            }
        },
        {
            (byte)eCharacterClass.Druid,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Druid,
                Name = "Druid",
                BaseClassID = (byte)eCharacterClass.Naturalist,
                ProfessionTranslationID = "PlayerClass.Profession.PathofHarmony",
                SpecPointMultiplier = 10,
                PrimaryStat = (int)eStat.EMP,
                SecondaryStat = (int)eStat.CON,
                TertiaryStat = (int)eStat.STR,
                ManaStat = (int)eStat.EMP,
                EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Troll.ID)}",
                BaseHP = 720,
                BaseWeaponSkill = 320,
                ClassType = (byte)eClassType.Hybrid,
            }
        },
        {
            (byte)eCharacterClass.Magician,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Magician,
                Name = "Magician",
                BaseClassID = (byte)eCharacterClass.Magician,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                ManaStat = (int)eStat.INT,
                EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Troll.ID)}",
                BaseHP = 560,
                BaseWeaponSkill = 280,
                ClassType = (byte)eClassType.ListCaster,
            }
        },
        {
            (byte)eCharacterClass.Guardian,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Guardian,
                Name = "Guardian",
                BaseClassID = (byte)eCharacterClass.Guardian,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Troll.ID)}",
                BaseHP = 880,
                BaseWeaponSkill = 400,
                ClassType = (byte)eClassType.PureTank,
            }
        },
        {
            (byte)eCharacterClass.Naturalist,
            new DBCharacterClass()
            {
                ID = (byte)eCharacterClass.Naturalist,
                Name = "Naturalist",
                BaseClassID = (byte)eCharacterClass.Naturalist,
                ProfessionTranslationID = "",
                SpecPointMultiplier = 10,
                ManaStat = (int)eStat.EMP,
                EligibleRaces = $"{(int)(PlayerRace.Celt.ID)},{(int)(PlayerRace.Troll.ID)}",
                BaseHP = 720,
                BaseWeaponSkill = 360,
                ClassType = (byte)eClassType.Hybrid,
            }
        }
    };
}
namespace Game.Logic.Inventory;

public enum eEquipmentItems : byte
{
    HORSE = 0x09,
    RIGHT_HAND = 0x0A,
    LEFT_HAND = 0x0B,
    TWO_HANDED = 0x0C,
    RANGED = 0x0D,
    HEAD = 0x15,
    HAND = 0x16,
    FEET = 0x17,
    JEWEL = 0x18,
    TORSO = 0x19,
    CLOAK = 0x1A,
    LEGS = 0x1B,
    ARMS = 0x1C,
    NECK = 0x1D,
    WAIST = 0x20,
    L_BRACER = 0x21,
    R_BRACER = 0x22,
    L_RING = 0x23,
    R_RING = 0x24,
    MYTHICAL = 0x25
};

public class Slot
{
    public const int HORSEARMOR = 7;
    public const int HORSEBARDING = 8;
    public const int HORSE = 9;
    public const int RIGHTHAND = 10;
    public const int LEFTHAND = 11;
    public const int TWOHAND = 12;
    public const int RANGED = 13;
    public const int FIRSTQUIVER = 14;
    public const int SECONDQUIVER = 15;
    public const int THIRDQUIVER = 16;
    public const int FOURTHQUIVER = 17;
    public const int HELM = 21;
    public const int HANDS = 22;
    public const int FEET = 23;
    public const int JEWELRY = 24;
    public const int TORSO = 25;
    public const int CLOAK = 26;
    public const int LEGS = 27;
    public const int ARMS = 28;
    public const int NECK = 29;
    public const int FOREARMS = 30;
    public const int SHIELD = 31;
    public const int WAIST = 32;
    public const int LEFTWRIST = 33;
    public const int RIGHTWRIST = 34;
    public const int LEFTRING = 35;
    public const int RIGHTRING = 36;
    public const int MYTHICAL = 37;
};

public enum eObjectType : byte
{
    GenericItem = 0,
    GenericWeapon = 1,

    //Albion weapons
    _FirstWeapon = 2,
    CrushingWeapon = 2,
    SlashingWeapon = 3,
    ThrustWeapon = 4,
    Fired = 5,
    TwoHandedWeapon = 6,
    PolearmWeapon = 7,
    Staff = 8,
    Longbow = 9,
    Crossbow = 10,
    Flexible = 24,

    //Midgard weapons
    Sword = 11,
    Hammer = 12,
    Axe = 13,
    Spear = 14,
    CompositeBow = 15,
    Thrown = 16,
    LeftAxe = 17,
    HandToHand = 25,

    //Hibernia weapons
    RecurvedBow = 18,
    Blades = 19,
    Blunt = 20,
    Piercing = 21,
    LargeWeapons = 22,
    CelticSpear = 23,
    Scythe = 26,

    //Mauler weapons
    FistWraps = 27,
    MaulerStaff = 28,
    _LastWeapon = 28,

    //Armor
    _FirstArmor = 31,
    GenericArmor = 31,
    Cloth = 32,
    Leather = 33,
    Studded = 34,
    Chain = 35,
    Plate = 36,
    Reinforced = 37,
    Scale = 38,
    _LastArmor = 38,

    //Misc
    Magical = 41,
    Shield = 42,
    Arrow = 43,
    Bolt = 44,
    Instrument = 45,
    Poison = 46,
    AlchemyTincture = 47,
    SpellcraftGem = 48,

    //housing
    _FirstHouse = 49,
    GardenObject = 49,
    HouseWallObject = 50,
    HouseFloorObject = 51,
    HouseCarpetFirst = 52,
    HouseNPC = 53,
    HouseVault = 54,
    HouseInteriorObject = 55, //Lathe, forge, alchemy table
    HouseTentColor = 56,
    HouseExteriorBanner = 57,
    HouseExteriorShield = 58,
    HouseRoofMaterial = 59,
    HouseWallMaterial = 60,
    HouseDoorMaterial = 61,
    HousePorchMaterial = 62,
    HouseWoodMaterial = 63,
    HouseShutterMaterial = 64,
    HouseInteriorBanner = 66,
    HouseInteriorShield = 67,
    HouseBindstone = 68,
    HouseCarpetSecond = 69,
    HouseCarpetThird = 70,
    HouseCarpetFourth = 71,
    _LastHouse = 71,

    //siege weapons
    SiegeBalista = 80, // need log
    SiegeCatapult = 81, // need log
    SiegeCauldron = 82, // need log
    SiegeRam = 83, // need log
    SiegeTrebuchet = 84, // need log
}

public enum eArmorSlot : int
{
    NOTSET = 0x00,
    HEAD = eInventorySlot.HeadArmor,
    HAND = eInventorySlot.HandsArmor,
    FEET = eInventorySlot.FeetArmor,
    TORSO = eInventorySlot.TorsoArmor,
    LEGS = eInventorySlot.LegsArmor,
    ARMS = eInventorySlot.ArmsArmor,
};

public abstract class ArmorLevel
{
    public const int GenericArmor = 0;
    public const int Cloth = 1;
    public const int Leather = 2;
    public const int Reinforced = 3;
    public const int Studded = 3;
    public const int Scale = 4;
    public const int Chain = 4;
    public const int Plate = 5;
}
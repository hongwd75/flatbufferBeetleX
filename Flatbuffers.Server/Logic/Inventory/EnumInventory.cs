namespace Game.Logic.Inventory;

	public enum eInventorySlot : int
	{
		Invalid           = 0,
		Ground            = 1,

		Min_Inv           = 7,
		HorseArmor        = 7, // Equipment, horse armor
		HorseBarding      = 8, // Equipment, horse barding
		Horse             = 9, // Equipment, horse

		MinEquipable	  = 10,
		RightHandWeapon   = 10,//Equipment, Visible
		LeftHandWeapon    = 11,//Equipment, Visible
		TwoHandWeapon     = 12,//Equipment, Visible
		DistanceWeapon    = 13,//Equipment, Visible
		FirstQuiver		  = 14,
		SecondQuiver	  = 15,
		ThirdQuiver		  = 16,
		FourthQuiver	  = 17,
		HeadArmor         = 21,//Equipment, Visible
		HandsArmor        = 22,//Equipment, Visible
		FeetArmor         = 23,//Equipment, Visible
		Jewellery         = 24,//Equipment
		TorsoArmor        = 25,//Equipment, Visible
		Cloak             = 26,//Equipment, Visible
		LegsArmor         = 27,//Equipment, Visible
		ArmsArmor         = 28,//Equipment, Visible
		Neck              = 29,//Equipment
		Waist             = 32,//Equipment
		LeftBracer        = 33,//Equipment
		RightBracer       = 34,//Equipment
		LeftRing          = 35,//Equipment
		RightRing         = 36,//Equipment
		Mythical		  = 37,
		MaxEquipable	  = 37,

		FirstBackpack     = 40,
		LastBackpack      = 79,
		
		FirstBagHorse	= 80,
		LastBagHorse	= 95,

		LeftFrontSaddleBag	= 96,
		RightFrontSaddleBag = 97,
		LeftRearSaddleBag	= 98,
		RightRearSaddleBag	= 99,

		PlayerPaperDoll   = 100,
	
		Mithril			  = 101,
		Platinum		  = 102,
		Gold			  = 103,
		Silver			  = 104,
		Copper			  = 105,
		
		FirstVault        = 110,
		LastVault         = 149,

		HousingInventory_First = 150,
		HousingInventory_Last = 249,	

		HouseVault_First = 1000,
		HouseVault_Last = 1399,
        Max_Inv = 249,
	}
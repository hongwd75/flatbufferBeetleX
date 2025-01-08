using Game.Logic.World;

namespace Game.Logic.CharacterClasses;

	public class PlayerRace
	{
		public eRace ID { get; }
		public virtual eGameDLC Expansion { get; }
		public eRealm Realm { get; }
		private eLivingModel FemaleModel { get; }
		private eLivingModel MaleModel { get; }

        private PlayerRace()
		{ 
			ID = eRace.Unknown;
		}

		private PlayerRace(eRace race, eRealm realm, eGameDLC expansion, eLivingModel maleModel, eLivingModel femaleModel)
        {
			ID = race;
			Realm = realm;
			Expansion = expansion;
			MaleModel = maleModel;
			FemaleModel = femaleModel;
        }

        public static PlayerRace Unknown { get; } = new PlayerRace();

		private static Dictionary<eRace, PlayerRace> races = new Dictionary<eRace, PlayerRace>()
		{
			{ eRace.Briton, new PlayerRace( eRace.Briton, eRealm.Albion, eGameDLC.Classic, eLivingModel.BritonMale, eLivingModel.BritonFemale) } ,
			{ eRace.Troll, new PlayerRace(eRace.Troll, eRealm.Midgard, eGameDLC.Classic, eLivingModel.TrollMale, eLivingModel.TrollFemale) },
			{ eRace.Celt, new PlayerRace(eRace.Celt, eRealm.Hibernia, eGameDLC.Classic, eLivingModel.CeltMale, eLivingModel.CeltFemale) } ,
		};

        public static PlayerRace GetRace(int id)
        {
            races.TryGetValue((eRace)id, out var race);
            if (race == null) return Unknown;
            return race;
        }

		public eLivingModel GetModel(eGender gender)
        {
			if (gender == eGender.Male) return MaleModel;
			else if (gender == eGender.Female) return FemaleModel;
			else return eLivingModel.None;
		}

		public static List<PlayerRace> AllRaces
		{
			get
			{
				var allRaces = new List<PlayerRace>();
				foreach (var race in races)
				{
					allRaces.Add(race.Value);
				}
				return allRaces;
			}
		}

		public static PlayerRace Briton => races[eRace.Briton];
		public static PlayerRace Troll => races[eRace.Troll];
		public static PlayerRace Celt => races[eRace.Celt];

        public override bool Equals(object obj)
        {
            if(obj is PlayerRace compareRace)
            {
				return compareRace.ID == ID;
            }
			return false;
        }

        public override int GetHashCode()
        {
			return (int)ID;
        }
    }

	public enum eGameDLC : byte
	{
		Classic = 1,
		Option1 = 2
	}
using System.Collections.Specialized;
using System.Reflection;
using Game.Logic.Currencys;
using Game.Logic.datatable;
using Game.Logic.World;
using log4net;
using Logic.database;
using Logic.database.table;
using NetworkMessage;

namespace Game.Logic.Guild;

public sealed class GuildMgr
{
	private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	private static readonly HybridDictionary m_guilds = new HybridDictionary();
	private static readonly HybridDictionary m_guildids = new HybridDictionary();
	private static readonly Dictionary<string, Dictionary<string, GuildMemberDisplay>> m_guildXAllMembers = new Dictionary<string, Dictionary<string, GuildMemberDisplay>>();
	
	public static Dictionary<string, GuildMemberDisplay> GetAllGuildMembers(string guildID)
	{
		if (m_guildXAllMembers.ContainsKey(guildID))
		{
			return new Dictionary<string, GuildMemberDisplay>(m_guildXAllMembers[guildID]);
		}

		return null;
	}

	public static void AddPlayerToAllGuildPlayersList(GamePlayer player)
	{
		if (m_guildXAllMembers.ContainsKey(player.GuildID))
		{
			if (!m_guildXAllMembers[player.GuildID].ContainsKey(player.InternalID))
			{
				Dictionary<string, GuildMemberDisplay> guildMemberList = m_guildXAllMembers[player.GuildID];
				GuildMemberDisplay member = new GuildMemberDisplay(	player.InternalID, 
																	player.Network.Account.Nickname, 
																	player.Level.ToString(), 
																	player.CharacterClass.ID.ToString(), 
																	player.GuildRank.RankLevel.ToString(), 
																	player.CurrentZone.Description);
				guildMemberList.Add(player.InternalID, member);
			}
		}
	}
	
	public static bool RemovePlayerFromAllGuildPlayersList(GamePlayer player)
	{
		if (m_guildXAllMembers.ContainsKey(player.GuildID))
		{
			return m_guildXAllMembers[player.GuildID].Remove(player.InternalID);
		}
		return false;
	}
	
	static private ushort m_lastID = 0;

	public const long COST_RE_EMBLEM = 1000000; //200 gold
	
	public static bool AddGuild(Guild guild)
	{
		if (guild == null)
			return false;

		lock (m_guilds.SyncRoot)
		{
			if (!m_guilds.Contains(guild.Name))
			{
				m_guilds.Add(guild.Name, guild);
				m_guildids.Add(guild.GuildID, guild.Name);
				guild.ID = ++m_lastID;
				return true;
			}
		}

		return false;
	}
	
	public static bool RemoveGuild(Guild guild)
	{
		if (guild == null)
			return false;

		guild.ClearOnlineMemberList();
		lock (m_guilds.SyncRoot)
		{
			m_guilds.Remove(guild.Name);
			m_guildids.Remove(guild.GuildID);
		}
		return true;
	}

	public static bool DoesGuildExist(string guildName)
	{
		lock (m_guilds.SyncRoot)
		{
            return m_guilds.Contains(guildName);
		}
	}
	
	public static Guild CreateGuild(eRealm realm, string guildName, GamePlayer creator = null)
	{
        if (DoesGuildExist(guildName))
        {
            if (creator != null)
                creator.Out.SendMessage(guildName + " already exists!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

            return null;
        }

        try
		{
			DBGuild dbGuild = new DBGuild();
			dbGuild.GuildName = guildName;
			dbGuild.GuildID = System.Guid.NewGuid().ToString();
			dbGuild.Realm = (byte)realm;
			Guild newguild = new Guild(dbGuild);
            if (newguild.AddToDatabase() == false)
            {
                if (creator != null)
                {
                    creator.Out.SendMessage("Database error, unable to add a new guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return null;
            }
            AddGuild(newguild);
            CreateRanks(newguild);
			
			if (log.IsDebugEnabled)
				log.Debug("Create guild; guild name=\"" + guildName + "\" Realm=" + GlobalConstants.RealmToName(newguild.Realm));

			return newguild;
		}
		catch (Exception e)
		{
			if (log.IsErrorEnabled) log.Error("CreateGuild", e);
			return null;
		}
	}

	public static void CreateRanks(Guild guild)
	{
		DBRank rank;
		for (int i = 0; i < 10; i++)
		{
			rank = CreateRank(guild, i);

			GameServer.Database.AddObject(rank);
			guild.Ranks[i] = rank;
		}
	}

	public static void RepairRanks(Guild guild)
	{
		DBRank rank;
		for (int i = 0; i < 10; i++)
		{
			bool foundRank = false;

			foreach (DBRank r in guild.Ranks)
			{
				if (r == null)
				{
					// I love DOLDB relations!
					break;
				}

				if (r.RankLevel == i)
				{
					foundRank = true;
					break;
				}
			}

			if (foundRank == false)
			{
				rank = CreateRank(guild, i);
				rank.Title = rank.Title.Replace("Rank", "Repaired Rank");
				GameServer.Database.AddObject(rank);
			}
		}
	}

	private static DBRank CreateRank(Guild guild, int rankLevel)
	{
		DBRank rank = new DBRank();
		rank.AcHear = false;
		rank.AcSpeak = false;
		rank.Alli = false;
		rank.Claim = false;
		rank.Emblem = false;
		rank.GcHear = true;
		rank.GcSpeak = false;
		rank.GuildID = guild.GuildID;
		rank.Invite = false;
		rank.OcHear = false;
		rank.OcSpeak = false;
		rank.Promote = false;
		rank.RankLevel = (byte)rankLevel;
		rank.Release = false;
		rank.Remove = false;
		rank.Title = "Rank " + rankLevel.ToString();
		rank.Upgrade = false;
		rank.View = false;
		rank.View = false;
		rank.Dues = false;

		if (rankLevel < 9)
		{
			rank.GcSpeak = true;
			rank.View = true;
		}
		if (rankLevel < 8)
		{
			rank.Emblem = true;
		}
		if (rankLevel < 7)
		{
			rank.AcHear = true;
		}
		if (rankLevel < 6)
		{
			rank.AcSpeak = true;
		}
		if (rankLevel < 5)
		{
			rank.OcHear = true;
		}
		if (rankLevel < 4)
		{
			rank.OcSpeak = true;
		}
		if (rankLevel < 3)
		{
			rank.Invite = true;
			rank.Promote = true;
		}
		if (rankLevel < 2)
		{
			rank.Release = true;
			rank.Upgrade = true;
			rank.Claim = true;
		}
		if (rankLevel < 1)
		{
			rank.Remove = true;
			rank.Alli = true;
			rank.Dues = true;
			rank.Withdraw = true;
			rank.Title = "Guildmaster";
			rank.Buff = true;
		}
		return rank;
	}

	public static bool DeleteGuild(string guildName)
	{
		try
		{
			Guild removeGuild = GetGuildByName(guildName);

			if (removeGuild == null)
				return false;

			var guilds = GameDB<DBGuild>.SelectObjects(DB.Column(nameof(DBGuild.GuildID)).IsEqualTo(removeGuild.GuildID));
			foreach (var guild in guilds)
			{
				foreach (var cha in GameDB<Account>.SelectObjects(DB.Column(nameof(Account.GuildID)).IsEqualTo(guild.GuildID)))
					cha.GuildID = "";
			}
			GameServer.Database.DeleteObject(guilds);

			var ranks = GameDB<DBRank>.SelectObjects(DB.Column(nameof(DBRank.GuildID)).IsEqualTo(removeGuild.GuildID));
			GameServer.Database.DeleteObject(ranks);

			lock (removeGuild.GetListOfOnlineMembers())
			{
				foreach (GamePlayer ply in removeGuild.GetListOfOnlineMembers())
				{
					ply.Guild = null;
					ply.GuildID = "";
					ply.GuildName = "";
					ply.GuildRank = null;
				}
			}

			RemoveGuild(removeGuild);

			return true;
		}
		catch (Exception e)
		{
			if (log.IsErrorEnabled)
				log.Error("DeleteGuild", e);
			return false;
		}
	}

	public static Guild GetGuildByName(string guildName)
	{
		if (guildName == null) return null;
		lock (m_guilds.SyncRoot)
		{
			return (Guild)m_guilds[guildName];
		}
	}

	public static Guild GetGuildByGuildID(string guildid)
	{
		if(guildid == null) return null;
		
		lock (m_guildids.SyncRoot)
		{
			if(m_guildids[guildid] == null) return null;
			
			lock(m_guilds.SyncRoot)
			{
				return (Guild)m_guilds[m_guildids[guildid]];
			}
		}
	}

	public static string GuildNameToGuildID(string guildName)
	{
		Guild g = GetGuildByName(guildName);
		if (g == null)
			return "";
		return g.GuildID;
	}

	public static bool LoadAllGuilds()
	{
		lock (m_guilds.SyncRoot)
		{
			m_guilds.Clear(); 
		}
		m_lastID = 0;

		//load guilds
		var guildObjs = GameServer.Database.SelectAllObjects<DBGuild>();
		foreach(var obj in guildObjs)
		{
			var myguild = new Guild(obj);

			if (obj.Ranks == null ||
			    obj.Ranks.Length < 10 ||
			    obj.Ranks[0] == null ||
			    obj.Ranks[1] == null ||
			    obj.Ranks[2] == null ||
			    obj.Ranks[3] == null ||
			    obj.Ranks[4] == null ||
			    obj.Ranks[5] == null ||
			    obj.Ranks[6] == null ||
			    obj.Ranks[7] == null ||
			    obj.Ranks[8] == null ||
			    obj.Ranks[9] == null)
			{
				log.ErrorFormat("GuildMgr: Ranks missing for {0}, creating new ones!", myguild.Name);

				RepairRanks(myguild);

				// now reload the guild to fix the relations
				myguild = new Guild(GameDB<DBGuild>.SelectObjects(DB.Column(nameof(DBGuild.GuildID)).IsEqualTo(obj.GuildID)).FirstOrDefault());
			}

			AddGuild(myguild);

			var guildCharacters = GameDB<Account>.SelectObjects(DB.Column(nameof(Account.GuildID)).IsEqualTo(myguild.GuildID));
			var tempList = new Dictionary<string, GuildMemberDisplay>(guildCharacters.Count);

			foreach (Account ch in guildCharacters)
			{
				var member = new GuildMemberDisplay(ch.ObjectId,
					ch.Nickname,
					"",
					"",
					ch.GuildRank.ToString(),
					"");
;
				tempList.Add(ch.ObjectId, member);
			}

			m_guildXAllMembers.Add(myguild.GuildID, tempList);
		}

		//load alliances
		var allianceObjs = GameServer.Database.SelectAllObjects<DBAlliance>();
		foreach (DBAlliance dball in allianceObjs)
		{
			var myalliance = new Alliance();
			myalliance.LoadFromDatabase(dball);

			if (dball != null && dball.DBguilds != null)
			{
				foreach (DBGuild mydbgui in dball.DBguilds)
				{
					var gui = GetGuildByName(mydbgui.GuildName);
					myalliance.Guilds.Add(gui);
					gui.alliance = myalliance;
				}
			}
		}
		return true;
	}

	public static void SaveAllGuilds()
	{
		if (log.IsDebugEnabled)
			log.Debug("Saving all guilds...");
		try
		{
			lock (m_guilds.SyncRoot)
			{
				foreach (Guild g in m_guilds.Values)
				{
					g.SaveIntoDatabase();
				}
			}
		}
		catch (Exception e)
		{
			if (log.IsErrorEnabled)
				log.Error("Error saving guilds.", e);
		}
	}

	public static bool IsEmblemUsed(int emblem)
	{
		lock (m_guilds.SyncRoot)
		{
			foreach (Guild guild in m_guilds.Values)
			{
				if (guild.Emblem == emblem)
					return true;
			}
		}
		return false;
	}

	public static void ChangeEmblem(GamePlayer player, int oldemblem, int newemblem)
	{
		player.Guild.Emblem = newemblem;
		if (oldemblem != 0)
		{
			player.RemoveMoney(Currency.Copper.Mint(COST_RE_EMBLEM));
            var objs = GameDB<InventoryItem>.SelectObjects(DB.Column(nameof(InventoryItem.Emblem)).IsEqualTo(oldemblem));
			
			foreach (InventoryItem item in objs)
			{
				item.Emblem = newemblem;
			}
			GameServer.Database.SaveObject(objs);
		}
	}

	public static List<Guild> GetAllGuilds()
	{
		var guilds = new List<Guild>(m_guilds.Count);

		lock (m_guilds.SyncRoot)
		{
			foreach (Guild guild in m_guilds.Values)
			{
				guilds.Add(guild);
			}
		}

		return guilds;
	}

	public class GuildMemberDisplay
	{
		#region Members

		string m_internalID;
		public string InternalID
		{
			get { return m_internalID; }
		}

		string m_name;
		public string Name
		{
			get { return m_name; }
		}

		string m_level;
		public string Level
		{
			get { return m_level; }
			set { m_level = value; }
		}

		string m_characterClassID;
		public string ClassID
		{
			get { return m_characterClassID; }
			set { m_characterClassID = value; }
		}

		string m_rank;
		public string Rank
		{
			get { return m_rank; }
			set { m_rank = value; }
		}

		string m_zoneOnline;
		public string ZoneOrOnline
		{
			get { return m_zoneOnline; }
			set { m_zoneOnline = value; }
		}
		#endregion

		public string this[eSocialWindowSortColumn i]
		{
			get
			{
				switch (i)
				{
					case eSocialWindowSortColumn.Name:
						return Name;
					case eSocialWindowSortColumn.ClassID:
						return ClassID;
					case eSocialWindowSortColumn.Level:
						return Level;
					case eSocialWindowSortColumn.Rank:
						return Rank;
					case eSocialWindowSortColumn.ZoneOrOnline:
						return ZoneOrOnline;
					default:
						return "";
				}
			}
		}

		public GuildMemberDisplay(string internalID, string name, string level, string classID, string rank,string zoneOrOnline)
		{
			m_internalID = internalID;
			m_name = name;
			m_level = level;
			m_characterClassID = classID;
			m_rank = rank;
			m_zoneOnline = zoneOrOnline;
		}

		public GuildMemberDisplay(GamePlayer player)
		{
			m_internalID = player.InternalID;
			m_name = player.Name;
			m_level = player.Level.ToString();
			m_characterClassID = player.CharacterClass.ID.ToString();
			m_rank = player.GuildRank.RankLevel.ToString(); ;
			m_zoneOnline = player.CurrentZone.ToString();
		}

		public string ToString(int position, int guildPop)
		{
			return string.Format("E,{0},{1},{2},{3},{4},{5},{6}",
			                     position, guildPop, m_name, m_level, m_characterClassID, m_rank,  m_zoneOnline);
		}

		public void UpdateMember(GamePlayer player)
		{
			Level = player.Level.ToString();
			ClassID = player.CharacterClass.ID.ToString();
			Rank = player.GuildRank.RankLevel.ToString();
			ZoneOrOnline = player.CurrentZone.Description;
		}

		public enum eSocialWindowSort : int
		{
			NameDesc = -1,
			NameAsc = 1,
			LevelDesc = -2,
			LevelAsc = 2,
			ClassDesc = -3,
			ClassAsc = 3,
			RankDesc = -4,
			RankAsc = 4,
			GroupDesc = -5,
			GroupAsc = 5,
			ZoneOrOnlineDesc = 6,
			ZoneOrOnlineAsc = -6,
			NoteDesc = 7,
			NoteAsc = -7
		}

		public enum eSocialWindowSortColumn : int
		{
			Name = 0,
			Level = 1,
			ClassID = 2,
			Rank = 3,
			ZoneOrOnline = 5,
		}
	}
}
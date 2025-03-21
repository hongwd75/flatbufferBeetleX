﻿using System.Reflection;
using Game.Logic.Currencys;
using Game.Logic.Language;
using Game.Logic.World;
using log4net;
using Logic.database.table;
using NetworkMessage;
using Money = Game.Logic.Utils.Money;

namespace Game.Logic.Guild;

public class Guild
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public enum eRank : int
		{
			Emblem,
			AcHear,
			AcSpeak,
			Demote,
			Promote,
			GcHear,
			GcSpeak,
			Invite,
			OcHear,
			OcSpeak,
			Remove,
			Leader,
			Alli,
			View,
			Claim,
			Upgrade,
			Release,
			Buff,
			Dues,
			Withdraw
		}

		public enum eBonusType : byte
		{
			None = 0,
			RealmPoints = 1,
			BountyPoints = 2,	// not live like?
			MasterLevelXP = 3,	// Not implemented
			CraftingHaste = 4,
			ArtifactXP = 5,
			Experience = 6
		}

		public static string BonusTypeToName(eBonusType bonusType)
		{
			string bonusName = "None";

			switch (bonusType)
			{
				case Guild.eBonusType.ArtifactXP:
					bonusName = "Artifact XP";
					break;
				case Guild.eBonusType.BountyPoints:
					bonusName = "Bounty Points";
					break;
				case Guild.eBonusType.CraftingHaste:
					bonusName = "Crafting Speed";
					break;
				case Guild.eBonusType.Experience:
					bonusName = "PvE Experience";
					break;
				case Guild.eBonusType.MasterLevelXP:
					bonusName = "Master Level XP";
					break;
				case Guild.eBonusType.RealmPoints:
					bonusName = "Realm Points";
					break;
			}

			return bonusName;
		}

		/// <summary>
		/// This holds all players inside the guild (InternalID, GamePlayer)
		/// </summary>
		protected readonly Dictionary<string, GamePlayer> m_onlineGuildPlayers = new Dictionary<string, GamePlayer>();

		/// <summary>
		/// Use this object to lock the guild member list
		/// </summary>
		public Object m_memberListLock = new Object();

		/// <summary>
		/// This holds all players inside the guild
		/// </summary>
		protected Alliance m_alliance = null;

		/// <summary>
		/// This holds the DB instance of the guild
		/// </summary>
		protected DBGuild m_DBguild;

		/// <summary>
		/// the runtime ID of the guild
		/// </summary>
		protected ushort m_id;

		/// <summary>
		/// Stores claimed keeps (unique)
		/// </summary>
		// protected List<AbstractGameKeep> m_claimedKeeps = new List<AbstractGameKeep>();

		public eRealm Realm
		{
			get
			{
				return (eRealm)m_DBguild.Realm;
			}
			set
			{
				m_DBguild.Realm = (byte)value;
			}
		}

		public string Webpage
		{
			get
			{
				return this.m_DBguild.Webpage;
			}
			set
			{
				this.m_DBguild.Webpage = value;
			}
		}

		public DBRank[] Ranks
		{
			get
			{
				return this.m_DBguild.Ranks;
			}
			set
			{
				this.m_DBguild.Ranks = value;
			}
		}

		public int GuildHouseNumber
		{
			get
			{
				if (m_DBguild.GuildHouseNumber == 0)
					m_DBguild.HaveGuildHouse = false;

				return m_DBguild.GuildHouseNumber;
			}
			set
			{
				m_DBguild.GuildHouseNumber = value;

				if (value == 0)
					m_DBguild.HaveGuildHouse = false;
				else
					m_DBguild.HaveGuildHouse = true;
			}
		}

		public bool GuildOwnsHouse
		{
			get
			{
				if (m_DBguild.GuildHouseNumber == 0)
					m_DBguild.HaveGuildHouse = false;

				return m_DBguild.HaveGuildHouse;
			}
			set
			{
				 m_DBguild.HaveGuildHouse = value;
			}
		}

		public double GetGuildBank()
		{
			return this.m_DBguild.Bank;
		}

		public bool IsGuildDuesOn()
		{
			return m_DBguild.Dues;
		}

		public long GetGuildDuesPercent()
		{
			return m_DBguild.DuesPercent;
		}

		public void SetGuildDues(bool dues)
		{
			m_DBguild.Dues = dues;
		}

		public void SetGuildDuesPercent(long dues)
		{
			if (IsGuildDuesOn() == true)
			{
				this.m_DBguild.DuesPercent = dues;
			}
			else
			{
				this.m_DBguild.DuesPercent = 0;
			}
		}
		/// <summary>
		/// Set guild bank command 
		/// </summary>
		/// <param name="donating"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public void SetGuildBank(GamePlayer donating, double amount)
		{
			if (donating == null || donating.Guild == null)
				return;

			if (amount < 0)
			{
				donating.Out.SendMessage(LanguageMgr.GetTranslation(donating.Network, "Scripts.Player.Guild.DepositInvalid"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				return;
			}
			else if ((donating.Guild.GetGuildBank() + amount) >= 1000000001)
			{
				donating.Out.SendMessage(LanguageMgr.GetTranslation(donating.Network, "Scripts.Player.Guild.DepositFull"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				return;
			}

            if (!donating.RemoveMoney(Currency.Copper.Mint(long.Parse(amount.ToString()))))
            {
                donating.Out.SendMessage("You don't have this amount of money !", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return;
            }

			donating.Out.SendMessage(LanguageMgr.GetTranslation(donating.Network, "Scripts.Player.Guild.DepositAmount", Money.GetString(long.Parse(amount.ToString()))), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);

			donating.Guild.UpdateGuildWindow();
			m_DBguild.Bank += amount;

            //InventoryLogging.LogInventoryAction(donating, "(GUILD;" + Name + ")", eInventoryActionType.Other, long.Parse(amount.ToString()));
            donating.Out.SendUpdatePlayer();
			return;
		}
		public void WithdrawGuildBank(GamePlayer withdraw, double amount)
		{
            if (amount < 0)
			{
				withdraw.Out.SendMessage(LanguageMgr.GetTranslation(withdraw.Network, "Scripts.Player.Guild.WithdrawInvalid"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				return;
			}
			else if ((withdraw.Guild.GetGuildBank() - amount) < 0)
			{
				withdraw.Out.SendMessage(LanguageMgr.GetTranslation(withdraw.Network, "Scripts.Player.Guild.WithdrawTooMuch"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
				return;
			}

            withdraw.Out.SendMessage(LanguageMgr.GetTranslation(withdraw.Network, "Scripts.Player.Guild.Withdrawamount", Money.GetString(long.Parse(amount.ToString()))), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
			withdraw.Guild.UpdateGuildWindow();
			m_DBguild.Bank -= amount;

		    var amt = long.Parse(amount.ToString());
            withdraw.AddMoney(Currency.Copper.Mint(amt));
            //InventoryLogging.LogInventoryAction("(GUILD;" + Name + ")", withdraw, eInventoryActionType.Other, amt);
            withdraw.Out.SendUpdatePlayer();
            withdraw.SaveIntoDatabase();
            withdraw.Guild.SaveIntoDatabase();
			return;
		}
		/// <summary>
		/// Creates an empty Guild. Don't use this, use
		/// GuildMgr.CreateGuild() to create a guild
		/// </summary>
		public Guild(DBGuild dbGuild)
		{
			this.m_DBguild = dbGuild;
			bannerStatus = "None";
		}

		public int Emblem
		{
			get
			{
				return this.m_DBguild.Emblem;
			}
			set
			{
				this.m_DBguild.Emblem = value;
				this.SaveIntoDatabase();
			}
		}

		public bool GuildBanner
		{
			get 
			{
				return this.m_DBguild.GuildBanner;
			}
			set
			{
				this.m_DBguild.GuildBanner = value;
				this.SaveIntoDatabase();
			}
		}

		public DateTime GuildBannerLostTime
		{
			get
			{
				return m_DBguild.GuildBannerLostTime;
			}
			set
			{
				this.m_DBguild.GuildBannerLostTime = value;
				this.SaveIntoDatabase();
			}
		}

		public string Omotd
		{
			get
			{
				return this.m_DBguild.oMotd;
			}
			set
			{
				this.m_DBguild.oMotd = value;
				this.SaveIntoDatabase();
			}
		}

		public string Motd
		{
			get
			{
				return this.m_DBguild.Motd;
			}
			set
			{
				this.m_DBguild.Motd = value;
				this.SaveIntoDatabase();
			}
		}

		public string AllianceId
		{
			get
			{
				return this.m_DBguild.AllianceID;
			}
			set
			{
				this.m_DBguild.AllianceID = value;
				this.SaveIntoDatabase();
			}
		}

		public Alliance alliance
		{
			get 
			{ 
				return m_alliance; 
			}
			set 
			{ 
				m_alliance = value; 
			}
		}

		public string GuildID
		{
			get 
			{ 
				return m_DBguild.GuildID; 
			}
			set 
			{
				m_DBguild.GuildID = value;
				this.SaveIntoDatabase();
			}
		}

		public ushort ID
		{
			get 
			{ 
				return m_id; 
			}
			set 
			{ 
				m_id = value; 
			}
		}

		public string Name
		{
			get 
			{ 
				return m_DBguild.GuildName; 
			}
			set 
			{
				m_DBguild.GuildName = value;
				this.SaveIntoDatabase();
			}
		}

		public long RealmPoints
		{
			get 
			{ 
				return this.m_DBguild.RealmPoints; 
			}
			set
			{
				this.m_DBguild.RealmPoints = value;
				this.SaveIntoDatabase();
			}
		}

		public long BountyPoints
		{
			get 
			{ 
				return this.m_DBguild.BountyPoints; 
			}
			set
			{
				this.m_DBguild.BountyPoints = value;
				this.SaveIntoDatabase();
			}
		}

		public int MemberOnlineCount
		{
			get
			{
				return m_onlineGuildPlayers.Count;
			}
		}

		public bool AddOnlineMember(GamePlayer player)
		{
			if(player==null) return false;
			lock (m_memberListLock)
			{
				if (!m_onlineGuildPlayers.ContainsKey(player.InternalID))
				{
					NotifyGuildMembers(player);
					m_onlineGuildPlayers.Add(player.InternalID, player);
					return true;
				}
			}

			return false;
		}

		private void NotifyGuildMembers(GamePlayer member)
		{
			foreach (GamePlayer player in m_onlineGuildPlayers.Values)
			{
				if (player == member) continue;
				if (player.Network.Account.ShowGuildLogins)
				{
					player.Out.SendMessage("Guild member " + member.Name + " has logged in!",
						NetworkMessage.eChatType.CT_System, NetworkMessage.eChatLoc.CL_SystemWindow);
				}
			}
		}

		public bool RemoveOnlineMember(GamePlayer player)
		{
			lock (m_memberListLock)
			{
				if (m_onlineGuildPlayers.ContainsKey(player.InternalID))
				{
					m_onlineGuildPlayers.Remove(player.InternalID);

					Dictionary<string, GuildMgr.GuildMemberDisplay> memberList = GuildMgr.GetAllGuildMembers(player.GuildID);

					if (memberList != null && memberList.ContainsKey(player.InternalID))
					{
						memberList[player.InternalID].ZoneOrOnline = DateTime.Now.ToShortDateString();
					}

					return true;
				}
			}
			return false;
		}

		public void ClearOnlineMemberList()
		{
			lock (m_memberListLock)
			{
				m_onlineGuildPlayers.Clear();
			}
		}

		public GamePlayer GetOnlineMemberByID(string memberID)
		{
			lock (m_memberListLock)
			{
				if (m_onlineGuildPlayers.ContainsKey(memberID))
					return m_onlineGuildPlayers[memberID];
			}

			return null;
		}

		public bool AddPlayer(GamePlayer addPlayer)
		{
			return AddPlayer(addPlayer, GetRankByID(9));
		}

		public bool AddPlayer(GamePlayer addPlayer, DBRank rank)
		{
			if (addPlayer == null || addPlayer.Guild != null)
				return false;
			
			if (log.IsDebugEnabled)
				log.Debug("Adding player to the guild, guild name=\"" + Name + "\"; player name=" + addPlayer.Name);

			try
			{
				AddOnlineMember(addPlayer);
				addPlayer.GuildName = Name;
				addPlayer.Network.Account.GuildID = GuildID;
				addPlayer.GuildRank = rank;
				addPlayer.Guild = this;
				addPlayer.SaveIntoDatabase();
				GuildMgr.AddPlayerToAllGuildPlayersList(addPlayer);
				addPlayer.Out.SendMessage("You have agreed to join " + this.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				addPlayer.Out.SendMessage("Your current rank is " + addPlayer.GuildRank.Title + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				SendMessageToGuildMembers(addPlayer.Name + " has joined the guild!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("AddPlayer", e);
				return false;
			}

			return true;
		}

		public bool RemovePlayer(string removername, GamePlayer member)
		{
			try
			{
				GuildMgr.RemovePlayerFromAllGuildPlayersList(member);
				RemoveOnlineMember(member);
				member.GuildName = "";
				member.Network.Account.GuildID = "";
				member.GuildRank = null;
				member.Guild = null;
				member.SaveIntoDatabase();
				GameServer.Database.SaveObject(member.Network.Account);
				/*
				 * ToDo. 길드 관련 데이터는 모두 account로 이동시켜야 한다. 
				 */
				member.Out.SendObjectGuildID(member, member.Guild);
				// Send message to removerClient about successful removal
				if (removername == member.Name)
					member.Out.SendMessage("You leave the guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				else
					member.Out.SendMessage(removername + " removed you from " + this.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("RemovePlayer", e);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Looks up if a given client have access for the specific command in this guild
		/// </summary>
		/// <returns>true or false</returns>
		public bool HasRank(GamePlayer member, Guild.eRank rankNeeded)
		{
			try
			{
				// Is the player in the guild at all?
				if (!m_onlineGuildPlayers.ContainsKey(member.InternalID))
				{
					log.Debug("Player " + member.Name + " (" + member.InternalID + ") is not a member of guild " + Name);
					return false;
				}

				// If player have a privlevel above 1, it has access enough
				if (member.Network.Account.PrivLevel > 1)
					return true;

                if (member.GuildRank == null)
                {
                    if (log.IsWarnEnabled)
                        log.Warn("Rank not in db for player " + member.Name);

                    return false;
                }

                switch (rankNeeded)
                {
                    case Guild.eRank.Emblem:
                        {
                            return member.GuildRank.Emblem;
                        }
                    case Guild.eRank.AcHear:
                        {
                            return member.GuildRank.AcHear;
                        }
                    case Guild.eRank.AcSpeak:
                        {
                            return member.GuildRank.AcSpeak;
                        }
                    case Guild.eRank.Demote:
                        {
                            return member.GuildRank.Promote;
                        }
                    case Guild.eRank.Promote:
                        {
                            return member.GuildRank.Promote;
                        }
                    case Guild.eRank.GcHear:
                        {
                            return member.GuildRank.GcHear;
                        }
                    case Guild.eRank.GcSpeak:
                        {
                            return member.GuildRank.GcSpeak;
                        }
                    case Guild.eRank.Invite:
                        {
                            return member.GuildRank.Invite;
                        }
                    case Guild.eRank.OcHear:
                        {
                            return member.GuildRank.OcHear;
                        }
                    case Guild.eRank.OcSpeak:
                        {
                            return member.GuildRank.OcSpeak;
                        }
                    case Guild.eRank.Remove:
                        {
                            return member.GuildRank.Remove;
                        }
                    case Guild.eRank.Alli:
                        {
                            return member.GuildRank.Alli;
                        }
                    case Guild.eRank.View:
                        {
                            return member.GuildRank.View;
                        }
                    case Guild.eRank.Claim:
                        {
                            return member.GuildRank.Claim;
                        }
                    case Guild.eRank.Release:
                        {
                            return member.GuildRank.Release;
                        }
                    case Guild.eRank.Upgrade:
                        {
                            return member.GuildRank.Upgrade;
                        }
                    case Guild.eRank.Dues:
                        {
                            return member.GuildRank.Dues;
                        }
                    case Guild.eRank.Withdraw:
                        {
                            return member.GuildRank.Withdraw;
                        }
                    case Guild.eRank.Leader:
                        {
                            return (member.GuildRank.RankLevel == 0);
                        }
                    case Guild.eRank.Buff:
                        {
                            return member.GuildRank.Buff;
                        }
                    default:
                        {
                            if (log.IsWarnEnabled)
                                log.Warn("Required rank not in the DB: " + rankNeeded);
                            return false;
                        }
                }
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("GotAccess", e);
				return false;
			}
		}

		/// <summary>
		/// get rank by level
		/// </summary>
		/// <param name="index">the index of rank</param>
		/// <returns>the dbrank</returns>
		public DBRank GetRankByID(int index)
		{
			try
			{
				foreach (DBRank rank in this.Ranks)
				{
					if (rank.RankLevel == index)
						return rank;

				}
				return null;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("GetRankByID", e);
				return null;
			}
		}

		/// <summary>
		/// Returns a list of all online members.
		/// </summary>
		/// <returns>ArrayList of members</returns>
		public IList<GamePlayer> GetListOfOnlineMembers()
		{
			return new List<GamePlayer>(m_onlineGuildPlayers.Values);
		}

		/// <summary>
		/// Sends a message to all guild members 
		/// </summary>
		/// <param name="msg">message string</param>
		/// <param name="type">message type</param>
		/// <param name="loc">message location</param>
		public void SendMessageToGuildMembers(string msg, eChatType type, eChatLoc loc)
		{
			lock (m_onlineGuildPlayers)
			{
				foreach (GamePlayer pl in m_onlineGuildPlayers.Values)
				{
					if (!HasRank(pl, Guild.eRank.GcHear))
					{
						continue;
					}
					pl.Out.SendMessage(msg, type, loc);
				}
			}
		}

		/// <summary>
		/// Called when this guild loose bounty points
		/// returns true if BPs were reduced and false if BPs are smaller than param amount
		/// if false is returned, no BPs were removed.
		/// </summary>
		public virtual bool RemoveBountyPoints(long amount)
		{
			if (amount > this.m_DBguild.BountyPoints)
			{
				return false;
			}
			this.m_DBguild.BountyPoints -= amount;
			this.SaveIntoDatabase();
			return true;
		}

		/// <summary>
		/// Gets or sets the guild merit points
		/// </summary>
		public long MeritPoints
		{
			get 
			{
				return this.m_DBguild.MeritPoints;
			}
			set 
			{
				this.m_DBguild.MeritPoints = value;
				this.SaveIntoDatabase();
			}
		}

		public long GuildLevel
		{
			get 
			{
				// added by Dunnerholl
				// props to valmerwolf for formula
				// checked with pendragon
				return (long)(Math.Sqrt(m_DBguild.RealmPoints / 10000) + 1);
			}
		}

		/// <summary>
		/// Gets or sets the guild buff type
		/// </summary>
		public eBonusType BonusType
		{
			get 
			{ 
				return (eBonusType)m_DBguild.BonusType; 
			}
			set 
			{
				this.m_DBguild.BonusType = (byte)value;
				this.SaveIntoDatabase();
			}
		}

		/// <summary>
		/// Gets or sets the guild buff time
		/// </summary>
		public DateTime BonusStartTime
		{
			get 
			{
				return this.m_DBguild.BonusStartTime; 
			}
			set 
			{
				this.m_DBguild.BonusStartTime = value;
				this.SaveIntoDatabase();
			}
		}

		public string Email
		{
			get
			{
				return this.m_DBguild.Email;
			}
			set
			{
				this.m_DBguild.Email = value;
				this.SaveIntoDatabase();
			}
		}

		/// <summary>
		/// Called when this guild gains merit points
		/// </summary>
		/// <param name="amount">The amount of bounty points gained</param>
		public virtual void GainMeritPoints(long amount)
		{
			MeritPoints += amount;
			UpdateGuildWindow();
		}

		/// <summary>
		/// Called when this guild loose bounty points
		/// </summary>
		/// <param name="amount">The amount of bounty points gained</param>
		public virtual void RemoveMeritPoints(long amount)
		{
			if (amount > MeritPoints)
				amount = MeritPoints;
			MeritPoints -= amount;
			UpdateGuildWindow();
		}

		public bool AddToDatabase()
		{
			return GameServer.Database.AddObject(this.m_DBguild);
		}
		/// <summary>
		/// Saves this guild to database
		/// </summary>
		public bool SaveIntoDatabase()
		{
			return GameServer.Database.SaveObject(m_DBguild);
		}

		private string bannerStatus;


		public void UpdateMember(GamePlayer player)
		{
			if (player.Guild != this)
				return;
			int housenum;
			if (player.Guild.GuildOwnsHouse)
			{
				housenum = player.Guild.GuildHouseNumber;
			}
			else
				housenum = 0;

			string mes = "I";
			mes += ',' + player.Guild.GuildLevel.ToString(); // Guild Level
			mes += ',' + player.Guild.GetGuildBank().ToString(); // Guild Bank money
			mes += ',' + player.Guild.GetGuildDuesPercent().ToString(); // Guild Dues enable/disable
			mes += ',' + player.Guild.BountyPoints.ToString(); // Guild Bounty
			mes += ',' + player.Guild.RealmPoints.ToString(); // Guild Experience
			mes += ',' + player.Guild.MeritPoints.ToString(); // Guild Merit Points
			mes += ',' + housenum.ToString(); // Guild houseLot ?
			mes += ',' + (player.Guild.MemberOnlineCount + 1).ToString(); // online Guild member ?
			mes += ",\"" + player.Guild.Motd + '\"'; // Guild Motd
			mes += ",\"" + player.Guild.Omotd + '\"'; // Guild oMotd
			player.Out.SendMessage(mes, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
			player.Guild.SaveIntoDatabase();
		}

		public void UpdateGuildWindow()
		{
			lock (m_onlineGuildPlayers)
			{
				foreach (GamePlayer player in m_onlineGuildPlayers.Values)
				{
					player.Guild.UpdateMember(player);
				}
			}
		}
	}
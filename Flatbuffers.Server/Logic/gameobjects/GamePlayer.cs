using System.Reflection;
using Game.Logic.CharacterClasses;
using Game.Logic.Currencys;
using Game.Logic.Effects;
using Game.Logic.Events;
using Game.Logic.Geometry;
using Game.Logic.Guild;
using Game.Logic.Language;
using Game.Logic.network;
using Game.Logic.World;
using Game.Logic.World.Timer;
using log4net;
using Logic.database;
using Logic.database.table;
using NetworkMessage;

namespace Game.Logic
{
    public class GamePlayer : GameLiving
    {
	    public const int PLAYER_BASE_SPEED = 191;
        private GameClient mNetwork = null;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object m_LockObject = new object();
        private Wallet Wallet { get; }
        
        protected DOLCharacters mdbCharacter;
        
        internal DOLCharacters DBCharacter
        {
            get { return mdbCharacter; }
        }
        
        public GameClient Network
        {
            get => mNetwork;
            set
            {
                mNetwork = value;
            }
        }

        public OutPacket Out => Network.Out;
        
        public void AddMoney(Money money) => Wallet.AddMoney(money);
        public bool RemoveMoney(Money money) => Wallet.RemoveMoney(money);
        
        public string ObjectId
        {
	        get { return DBCharacter != null ? DBCharacter.ObjectId : InternalID; }
	        set { if (DBCharacter != null) DBCharacter.ObjectId = value; }
        }
        
        public virtual CharacterClass CharacterClass { get; protected set; }

        public string Salutation => CharacterClass.GetSalutation(Gender);
      
        public string AccountName
        {
	        get { return DBCharacter != null ? DBCharacter.AccountName : string.Empty; }
        }
        
        public override eGender Gender
        {
	        get
	        {
		        if (DBCharacter.Gender == 0)
		        {
			        return eGender.Male;
		        }

		        return eGender.Female;
	        }
	        set
	        {
	        }
        }
        
        public virtual string LastName
        {
	        get { return DBCharacter != null ? DBCharacter.LastName : string.Empty; }
	        set
	        {
		        if (DBCharacter == null) return;
		        DBCharacter.LastName = value;
		        //update last name for all players if client is playing
		        if (ObjectState == eObjectState.Active)
		        {
			        Out.SendUpdatePlayer();
			        foreach (GamePlayer player in GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
			        {
				        if (player == null) continue;
				        if (player != this)
				        {
					        player.Out.SendObjectRemove(this);
					        player.Out.SendPlayerCreate(this);
					        player.Out.SendLivingEquipmentUpdate(this);
				        }
			        }
		        }
	        }
        }


        #region Guild
        private Guild.Guild m_guild;
        private DBRank m_guildRank;
        
        public Guild.Guild Guild
        {
	        get { return m_guild; }
	        set
	        {
		        if (value == null)
		        {
			        m_guild.RemoveOnlineMember(this);
		        }

		        m_guild = value;
		        if (ObjectState == eObjectState.Active)
		        {
			        Out.SendUpdatePlayer();
			        foreach (GamePlayer player in GetPlayersInRadius(WorldManager.VISIBILITY_DISTANCE))
			        {
				        if (player == null) continue;
				        if (player != this)
				        {
					        player.Out.SendObjectRemove(this);
					        player.Out.SendPlayerCreate(this);
					        player.Out.SendLivingEquipmentUpdate(this);
				        }
			        }
		        }
	        }
        }
        
        public DBRank GuildRank
        {
	        get { return m_guildRank; }
	        set
	        {
		        m_guildRank = value;
		        if (value != null && DBCharacter != null)
		        {
			        DBCharacter.GuildRank = value.RankLevel;
		        }
	        }
        }

        /// <summary>
        /// Gets or sets the database guildid of this player
        /// (delegate to DBCharacter)
        /// </summary>
        public string GuildID
        {
	        get { return Network.Account.GuildID; }
        }
        #endregion

        #region Sprint
        protected SprintEffect m_sprintEffect = null;
        /// <summary>
        /// Gets sprinting flag
        /// </summary>
        public bool IsSprinting
        {
	        get { return m_sprintEffect != null; }
        }
        /// <summary>
        /// Change sprint state of this player
        /// </summary>
        /// <param name="state">new state</param>
        /// <returns>sprint state after command</returns>
        public virtual bool Sprint(bool state)
        {
	        if (state == IsSprinting)
		        return state;

	        if (state)
	        {
		        // can't start sprinting with 10 endurance on 1.68 server
		        if (Endurance <= 10)
		        {
			        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.TooFatigued"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			        return false;
		        }
		        if (IsStealthed)
		        {
			        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.CantSprintHidden"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			        return false;
		        }
		        if (!IsAlive)
		        {
			        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.CantSprintDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			        return false;
		        }

		        m_sprintEffect = new SprintEffect();
		        m_sprintEffect.Start(this);
		        Out.SendUpdateMaxSpeed();
		        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.PrepareSprint"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		        return true;
	        }
	        else
	        {
		        m_sprintEffect.Stop();
		        m_sprintEffect = null;
		        Out.SendUpdateMaxSpeed();
		        Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Sprint.NoLongerReady"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
		        return false;
	        }
        }
        

        #endregion
        
        public Position BindPosition
        {
            get
            {
                if(DBCharacter == null) return Position.Zero;
                return DBCharacter.GetBindPosition();
            }
            set
            {
                if (DBCharacter == null) return;

                DBCharacter.BindRegion = value.RegionID;
                DBCharacter.BindXpos = value.X;
                DBCharacter.BindYpos = value.Y;
                DBCharacter.BindZpos = value.Z;
                DBCharacter.BindHeading = value.Orientation.InHeading;
            }
        }
        
        public virtual bool MoveToBind()
        {
            Region rgn = WorldManager.GetRegion(BindPosition.RegionID);
            if (rgn == null || rgn.GetZone(BindPosition.Coordinate) == null)
            {
                Network?.Out.SendPlayerQuit(true);
                SaveIntoDatabase();
                Quit(true);

                //if (ServerProperties.Properties.BAN_HACKERS)
                {
                    DBBannedAccount b = new DBBannedAccount();
                    b.Author = "SERVER";

                    b.Ip = Network?.TcpEndpointAddress ?? "";
                    b.Account = AccountName;
                    b.DateBan = DateTime.Now;
                    b.Type = "B";
                    b.Reason = "X/Y/RegionID : " + Position.X + "/" + Position.Y + "/" + Position.RegionID;
                    GameServer.Database.AddObject(b);
                    GameServer.Database.SaveObject(b);
                    
                    if (Network != null && Network.IsConnected() == true)
                    {
	                    string message = "Unknown bind point, your account is banned, contact a GM.";
	                    Network.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_SystemWindow);
	                    Network.Out.SendMessage(message, eChatType.CT_Help, eChatLoc.CL_ChatWindow);
                    }
                }
                return false;
            }
            return MoveTo(BindPosition);
        }

		public virtual bool Quit(bool forced)
		{
			if (!forced)
			{
				if (!IsAlive)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.CantQuitDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (IsMoving)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.CantQuitStanding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				if (CurrentRegion.IsInstance)
				{
                    Out.SendMessage(LanguageMgr.GetTranslation(Network.Account.Language, "GamePlayer.Quit.CantQuitInInstance"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				if (Statistics != null)
				{
					string stats = Statistics.GetStatisticsMessage();
					if (stats != "")
					{
						Out.SendMessage(stats, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}

				if (!IsSitting)
				{
					Sit(true);
				}
				int secondsleft = QuitTime;

				if (m_quitTimer == null)
				{
					m_quitTimer = new RegionTimer(this);
					m_quitTimer.Callback = new RegionTimerCallback(QuitTimerCallback);
					m_quitTimer.Start(1);
				}

				if (secondsleft > 20)
					Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.RecentlyInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Out.SendMessage(LanguageMgr.GetTranslation(Client.Account.Language, "GamePlayer.Quit.YouWillQuit2", secondsleft), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				//Notify our event handlers (if any)
				Notify(GamePlayerEvent.Quit, this);

				// log quit
				AuditMgr.AddAuditEntry(Client, AuditType.Character, AuditSubtype.CharacterLogout, "", Name);

				//Cleanup stuff
				Delete();
			}
			return true;
		}
		
        protected virtual void CleanupOnDisconnect()
        {
	        StopAttack();
	        Stealth(false);
	        try
	        {
		        EffectList.SaveAllEffects();
		        CancelAllConcentrationEffects();
		        EffectList.CancelAll();
	        }
	        catch (Exception e)
	        {
		        log.ErrorFormat("Cannot cancel all effects - {0}", e);
	        }	        
        }

        public override void Delete()
        {
	        //Todo. 데이터 제거
        }

        #region Invulnerability
		public delegate void InvulnerabilityExpiredCallback(GamePlayer player);
		protected InvulnerabilityTimer m_invulnerabilityTimer;
		protected long m_invulnerabilityTick;
		public virtual bool StartInvulnerabilityTimer(int duration, InvulnerabilityExpiredCallback callback)
		{
			if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvE)
				return false;

			if (duration < 1)
            {
	            return false;
            }

			long newTick = CurrentRegion.Time + duration;
			if (newTick < m_invulnerabilityTick)
				return false;

			m_invulnerabilityTick = newTick;
			if (m_invulnerabilityTimer != null)
				m_invulnerabilityTimer.Stop();

			if (callback != null)
			{
				m_invulnerabilityTimer = new InvulnerabilityTimer(this, callback);
				m_invulnerabilityTimer.Start(duration);
			}
			else
			{
				m_invulnerabilityTimer = null;
			}

			return true;
		}

		public virtual bool IsInvulnerableToAttack
		{
			get { return m_invulnerabilityTick > CurrentRegion.Time; }
		}

		protected class InvulnerabilityTimer : RegionAction
		{
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
			private readonly InvulnerabilityExpiredCallback m_callback;

			public InvulnerabilityTimer(GamePlayer actionSource, InvulnerabilityExpiredCallback callback)
				: base(actionSource)
			{
				if (callback == null)
					throw new ArgumentNullException("callback");
				m_callback = callback;
			}

			protected override void OnTick()
			{
				try
				{
					m_callback((GamePlayer)m_actionSource);
				}
				catch (Exception e)
				{
					log.Error("InvulnerabilityTimer callback", e);
				}
			}
		}
		#endregion

		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			if (!(obj is DOLCharacters))
			{
				return;
			}

			mdbCharacter = (DOLCharacters)obj;
			
			Wallet.InitializeFromDatabase();
			
			Model = (ushort)DBCharacter.CurrentModel;
			
			
		}

		public GamePlayer(GameClient client, DOLCharacters dbChar)
			: base()
		{
			Wallet = new Wallet(this);
			mNetwork = client;
			mdbCharacter = dbChar;
			
			#region guild handling ================================================
			var guildid = client.Account.GuildID;
			if (guildid != null)
				m_guild = GuildMgr.GetGuildByGuildID(guildid);
			else
				m_guild = null;

			if (m_guild != null)
			{
				foreach (DBRank rank in m_guild.Ranks)
				{
					if (rank == null) continue;
					if (rank.RankLevel == DBCharacter.GuildRank)
					{
						m_guildRank = rank;
						break;
					}
				}

				m_guildName = m_guild.Name;
				m_guild.AddOnlineMember(this);
			}
			#endregion			
		}
    }
}
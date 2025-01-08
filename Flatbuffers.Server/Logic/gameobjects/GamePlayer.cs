using System.Reflection;
using Game.Logic.Geometry;
using Game.Logic.network;
using Game.Logic.World;
using log4net;
using Logic.database.table;
using NetworkMessage;

namespace Game.Logic
{
    public class GamePlayer : GameLiving
    {
        private GameClient mNetwork = null;
        private string mAccountName = null;
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
        
        public string ObjectId
        {
	        get { return DBCharacter != null ? DBCharacter.ObjectId : InternalID; }
	        set { if (DBCharacter != null) DBCharacter.ObjectId = value; }
        }
        
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
                // if (log.IsErrorEnabled)
                //     log.Error("Player: " + Name + " unknown bind point : (R/X/Y) " + BindPosition.RegionID + "/" + BindPosition.X + "/" + BindPosition.Y);
                //Kick the player, avoid server freeze
                Client.Out.SendPlayerQuit(true);
                SaveIntoDatabase();
                Quit(true);
                //now ban him
                if (ServerProperties.Properties.BAN_HACKERS)
                {
                    DBBannedAccount b = new DBBannedAccount();
                    b.Author = "SERVER";

                    b.Ip = Network?.TcpEndpointAddress ?? "";
                    b.Account = this.mAccountName;
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
        
		#region Invulnerability

		/// <summary>
		/// The delegate for invulnerability expire callbacks
		/// </summary>
		public delegate void InvulnerabilityExpiredCallback(GamePlayer player);
		/// <summary>
		/// Holds the invulnerability timer
		/// </summary>
		protected InvulnerabilityTimer m_invulnerabilityTimer;
		/// <summary>
		/// Holds the invulnerability expiration tick
		/// </summary>
		protected long m_invulnerabilityTick;

		/// <summary>
		/// Starts the Invulnerability Timer
		/// </summary>
		/// <param name="duration">The invulnerability duration in milliseconds</param>
		/// <param name="callback">
		/// The callback for when invulnerability expires;
		/// not guaranteed to be called if overwriten by another invulnerability
		/// </param>
		/// <returns>true if invulnerability was set (smaller than old invulnerability)</returns>
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

		public GamePlayer(GameClient client, DOLCharacters dbChar)
			: base()
		{
			mNetwork = client;
			mdbCharacter = dbChar;
			mAccountName = client.Account.Name;
			
			m_buffMultBonus
		}
    }
}
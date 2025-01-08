using System.Net;
using System.Net.Sockets;
using BeetleX;
using Flatbuffers.Messages.Packets.Client;
using Game.Logic;
using Logic.database.table;

namespace Game.Logic.network
{
    public class GameClient 
    {
        #region eClientState enum

        public enum eClientState
        {
            NotConnected = 0x00,
            Connecting = 0x01,
            CharScreen = 0x02,
            WorldEnter = 0x03,
            Playing = 0x04,
            Linkdead = 0x05,
            Disconnected = 0x06,
        };
        #endregion

        protected ISession mSession;
        protected GamePlayer mPlayer;
        protected Account mAccount;
        protected int mGamePlayerArrayID;
        
        protected long mPingTime = DateTime.Now.Ticks;
        protected volatile eClientState m_clientState = eClientState.NotConnected;

        protected long mRoomID = 0;      // room 형식의 게임용 room id
        
        #region GET / SET

        public long RoomID 
        { 
            get => mRoomID;
            set { mRoomID = value; }
        }
        
        public OutPacket Out
        {
            get
            {
                if (mSession != null && mSession.IsDisposed == false)
                {
                    return (OutPacket)mSession.SocketProcessHandler;
                }
                return null;
            }
        }
        public GamePlayer Player
        {
            get => mPlayer;
            set { mPlayer = value; }
        }

        public ISession Session
        {
            get => mSession;
            set { mSession = value; }
        }

        public int PlayerArrayID
        {
            get => mGamePlayerArrayID;
            set { mGamePlayerArrayID = value; }
        }

        public Account Account
        {
            get => mAccount;
            set { mAccount = value; }
        }

        public eClientState ClientState
        {
            get { return m_clientState; }
            set
            {
                eClientState oldState = m_clientState;

                // refresh ping timeouts immediately when we change into playing state or charscreen
                if ((oldState != eClientState.Playing && value == eClientState.Playing) ||
                    (oldState != eClientState.CharScreen && value == eClientState.CharScreen))
                {
                    PingTime = DateTime.Now.Ticks;
                }

                m_clientState = value;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return m_clientState == eClientState.Playing;
            }
        }
        
        public long PingTime
        {
            get { return mPingTime; }
            set { mPingTime = value; }
        }

        public bool IsConnectedBan = false;
        #endregion

        public bool IsConnected()
        {
            return Session != null && Session.IsDisposed == false && m_clientState > eClientState.NotConnected;
        }
        
        public string TcpEndpointAddress
        {
            get
            {
                Socket s = Session?.Socket;
                if (s != null && s.Connected && s.RemoteEndPoint != null)
                    return ((IPEndPoint) s.RemoteEndPoint).Address.ToString();

                return "not connected";
            }
        }
        
        public GameClient(ISession session)
        {
            mSession = session;
        }


        public void Send<T>(ClientPacket id, T message)
        {
        }
    }
}
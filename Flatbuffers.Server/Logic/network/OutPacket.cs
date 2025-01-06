using System.Net.Sockets;
using BeetleX;
using Flatbuffers.Messages;
using Flatbuffers.Messages.Enums;
using Google.FlatBuffers;
using NetworkMessage;

namespace Game.Logic.network
{
    public abstract class OutPacket : ISessionSocketProcessHandler
    {
        private ISession Session = null;
        private GameClient mGameClient = null;

        public GameClient Client
        {
            get => mGameClient;
            set
            {
                mGameClient = value;
            }
        }
        
        public OutPacket(ISession session)
        {
            Session = session;
        }

        protected void Send(ServerPackets sc, byte[] buffer)
        {
            if (Session != null)
            {
                Session.Send(((ushort)sc, buffer));
            }
        }
        
        public void ReceiveCompleted(ISession session, SocketAsyncEventArgs e)
        {
        }

        public void SendCompleted(ISession session, SocketAsyncEventArgs e)
        {
        }

        //=======================================================================================================
        // 생성 패킷
        public abstract void SendLoginDenied(eLoginError error);
        public abstract void SendLoginInfo();
        public abstract void SendTime();

    }
}
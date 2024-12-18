using System.Net.Sockets;
using BeetleX;
using Flatbuffers.Messages;
using Flatbuffers.Messages.Enums;
using Google.FlatBuffers;
using NetworkMessage;

namespace Game.Logic.network
{
    public class OutPacket : ISessionSocketProcessHandler
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
        
        // 생성 패킷
        public void SendLoginDenied(eLoginError error)
        {
            //lock (sendBuilder)
            {
                FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
                SC_LoginAns_FBS req = new SC_LoginAns_FBS();
                req.Errorcode = (int)error;
                var packfunc = GameServer.SendPacketClassMethods.GetServerPacketType(ServerPackets.SC_LoginAns, req);
                object packedOffset = packfunc.method.Invoke(packfunc.obj, new object[] { sendBuilder, req });
                sendBuilder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
                Send(ServerPackets.SC_LoginAns, sendBuilder.SizedByteArray());
            }
        }

        public void SendLoginInfo()
        {
            //lock (sendBuilder)
            {
                FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
                SC_LoginAns_FBS req = new SC_LoginAns_FBS();
                req.Errorcode = 0;
                req.Nickname = Client.Account.Name;
                req.Sessionid = Client.PlayerArrayID;
                var packfunc = GameServer.SendPacketClassMethods.GetServerPacketType(ServerPackets.SC_LoginAns, req);
                object packedOffset = packfunc.method.Invoke(packfunc.obj, new object[] { sendBuilder, req });
                sendBuilder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
                Send(ServerPackets.SC_LoginAns, sendBuilder.SizedByteArray());
            }
        }

        public void ReceiveCompleted(ISession session, SocketAsyncEventArgs e)
        {
        }

        public void SendCompleted(ISession session, SocketAsyncEventArgs e)
        {
        }
    }
}
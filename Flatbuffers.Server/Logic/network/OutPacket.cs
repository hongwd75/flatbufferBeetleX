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
        
        FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);

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

        protected void Send(ServerPackets sc, ByteBuffer buffer)
        {
            PacketData obj = new PacketData()
            {
                ID = (ushort)sc,
                Data = buffer
            };
            Session.Send(obj);
        }
        
        // 생성 패킷
        public void SendLoginDenied(eLoginError error)
        {
            lock (sendBuilder)
            {
                sendBuilder.Clear();

                SC_LoginAns_FBS req = new SC_LoginAns_FBS();
                req.Errorcode = (int)error;
                var packedOffset = GameServer.SendPacketClassMethods.GetServerPacketType(ServerPackets.SC_LoginAns, req);
                sendBuilder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
                Send(ServerPackets.SC_LoginAns, sendBuilder.DataBuffer);
            }
        }

        public void SendLoginInfo()
        {
            lock (sendBuilder)
            {
                sendBuilder.Clear();
                SC_LoginAns_FBS req = new SC_LoginAns_FBS();
                req.Errorcode = 0;
                req.Nickname = Client.Account.Name;
                req.Sessionid = Client.PlayerArrayID;
                var packedOffset = GameServer.SendPacketClassMethods.GetServerPacketType(ServerPackets.SC_LoginAns, req);
                sendBuilder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
                Send(ServerPackets.SC_LoginAns, sendBuilder.DataBuffer);
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
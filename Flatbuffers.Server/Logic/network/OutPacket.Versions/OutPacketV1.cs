using BeetleX;
using Flatbuffers.Messages.Enums;
using Google.FlatBuffers;
using NetworkMessage;

namespace Game.Logic.network
{
    public class OutPacketV1 : OutPacket
    {
        public OutPacketV1(ISession session) : base(session)
        {
        }
        
        // 생성 패킷
        public override void SendLoginDenied(eLoginError error)
        {
            FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
            SC_LoginAns_FBS req = new SC_LoginAns_FBS();
            req.Errorcode = (int)error;
            
            var packfunc = GameServer.SendPacketClassMethods.GetServerPacketType(ServerPackets.SC_LoginAns, req);
            object packedOffset = packfunc.method.Invoke(packfunc.obj, new object[] { sendBuilder, req });
            sendBuilder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
            Send(ServerPackets.SC_LoginAns, sendBuilder.SizedByteArray());
        }

        public override void SendLoginInfo()
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

        public override void SendTime()
        {
           // 12시가 넘음 ㅇㅇ
        }

        public override void SendMessage(string message, eChatType type, eChatLoc loc)
        {
            FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
            SC_StringMessage_FBS req = new SC_StringMessage_FBS();
            req.Seesionid = Client.PlayerArrayID;
            req.Chatloc = loc;
            req.Chattype = type;
            req.Message = message;
            
            var packfunc = GameServer.SendPacketClassMethods.GetServerPacketType(ServerPackets.SC_StringMessage, req);
            object packedOffset = packfunc.method.Invoke(packfunc.obj, new object[] { sendBuilder, req });
            sendBuilder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
            Send(ServerPackets.SC_StringMessage, sendBuilder.SizedByteArray());            
        }
    }
}
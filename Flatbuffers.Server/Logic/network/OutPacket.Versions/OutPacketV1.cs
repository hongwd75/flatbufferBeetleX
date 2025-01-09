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
        

        public override void SendTime()
        {
            // 12시가 넘음 ㅇㅇ
        }
        
        // 생성 패킷
        public override void SendLoginDenied(eLoginError error)
        {
            FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
            SC_LoginAns_FBS req = new SC_LoginAns_FBS()
            {
                Errorcode = (int)error
            };
            
            SendFlatBufferPacket(ServerPackets.SC_LoginAns, sendBuilder, req);
        }

        public override void SendLoginInfo()
        {
            FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
            SC_LoginAns_FBS req = new SC_LoginAns_FBS()
            {
                Errorcode = 0,
                Nickname = Client.Account.Name,
                Sessionid = Client.PlayerArrayID
            };
            
            SendFlatBufferPacket(ServerPackets.SC_LoginAns, sendBuilder, req);
        }

        public override void SendPlayerQuit(bool totalOut)
        {
            FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
            SC_Quit_FBS req = new SC_Quit_FBS()
            {
                Totalout = totalOut,
                Level = Client == null ? (byte)0 : Client.Player.Level
            };
            
            SendFlatBufferPacket(ServerPackets.SC_Quit, sendBuilder, req);
        }
        public override void SendMessage(string message, eChatType type, eChatLoc loc)
        {
            FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
            SC_StringMessage_FBS req = new SC_StringMessage_FBS()
            {
                Seesionid = Client.PlayerArrayID,
                Chatloc = loc,
                Chattype = type,
                Message = message
            };

            SendFlatBufferPacket(ServerPackets.SC_StringMessage, sendBuilder, req);
        }

        public override void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4,
            eDialogType type,
            bool autoWrapText, string message)
        {
            FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
            SC_DialogBoxMessage_FBS req = new SC_DialogBoxMessage_FBS()
            {
                Code = code,
                Autowraptext = autoWrapText,
                Type = type,
                Data1 = data1,
                Data2 = data2,
                Data3 = data3,
                Data4 = data4,
                Message = message
            };
            SendFlatBufferPacket(ServerPackets.SC_DialogBoxMessage, sendBuilder, req);
        }
    }
}
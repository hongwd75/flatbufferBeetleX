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
        protected void SendFlatBufferPacket<T>(ServerPackets packetType, FlatBufferBuilder builder, T request)
            where T : class
        {
            var packfunc = GameServer.SendPacketClassMethods.GetServerPacketType(packetType, request);
            object packedOffset = packfunc.method.Invoke(packfunc.obj, new object[] { builder, request });
            builder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
            Send(packetType, builder.SizedByteArray());
        }
        
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
        public abstract void SendPlayerQuit(bool totalOut);
        public abstract void SendMessage(string message, eChatType type, eChatLoc loc);
        public abstract void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type,
            bool autoWrapText, string message);        

    }
}
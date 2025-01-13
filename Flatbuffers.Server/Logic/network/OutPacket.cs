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

        public virtual void OnDisconnect()
        {
            Session = null;
            if (Client != null)
            {
                if (Client.PlayerArrayID >= 0)
                {
                    if (Client.ClientState != GameClient.eClientState.Playing)
                    {
                        // 게임중이면 슬롯에서 빼지 않고, 게임 완료 후, 슬롯에서 제거
                        GameServer.Instance.Clients.Remove(Client.PlayerArrayID);
                    }
                }
                // Todo. 플레이어 내부 처리 추가 필요
            }
        }

        //=======================================================================================================
        // 생성 패킷
        public abstract void SendTime();
        public abstract void SendLoginDenied(eLoginError error);
        public abstract void SendLoginInfo();
        public abstract void SendObjectUpdate(GameObject obj);
        public abstract void SendLivingDataUpdate(GameLiving living, bool updateStrings);
        public abstract void SendPlayerQuit(bool totalOut);
        public abstract void SendMessage(string message, eChatType type, eChatLoc loc);
        public abstract void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type,
            bool autoWrapText, string message);        

    }
}
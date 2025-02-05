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
        public delegate void CheckLOSMgrResponse(GamePlayer player, ushort response, ushort sourceOID, ushort targetOID);
        
        // 에니메이션 인덱스
        public virtual int OneDualWeaponHit => 0x1f5;
        public virtual int BothDualWeaponHit => 0x1f6;
        
        protected void SetFlag(ref byte flags, bool condition, byte value)
        {
            if (condition)
            {
                flags |= value;
            }
        }        
        
        protected void SendFlatBufferPacket<T>(ServerPackets packetType, FlatBufferBuilder builder, T request)
            where T : class
        {
            var packfunc = GameServer.SendPacketClassMethods.GetServerPacketType(packetType, request);
            object packedOffset = packfunc.method.Invoke(packfunc.obj, new object[] { builder, request });
            builder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
            Send(packetType, builder.SizedByteArray());
        }

        protected void SendFlatBufferPacket<T>(ServerPackets packetType, T request)
            where T : class
        {
            FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
            SendFlatBufferPacket(packetType, sendBuilder, request);
        }

        protected GamePlayer Player => Client.Player;
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

        public void SendModelAndSizeChange(GameObject obj, ushort newModel, byte newSize)
        {
            SendModelAndSizeChange((ushort) obj.ObjectID, newModel, newSize);
        }
        
        public void SendModelChange(GameObject obj, ushort newModel)
        {
            if (obj is GameNPC)
                SendModelAndSizeChange(obj, newModel, (obj as GameNPC).Size);
            else
                SendModelAndSizeChange(obj, newModel, 0);
        }
        
        //=======================================================================================================
        // ** 빈 함수들 **
        //=======================================================================================================
        public virtual void SendTrainerWindow()
        {
            
        }
        
        //=======================================================================================================
        // ** 생성 패킷 **
        //=======================================================================================================
        public abstract void SendTime();
        public abstract void SendNPCCreate(GameNPC npc);
        public abstract void SendObjectCreate(GameObject obj);
        public abstract void SendMovingObjectCreate(GameMovingObject obj);
        public abstract void SendLoginDenied(eLoginError error);
        public abstract void SendLoginInfo();
        public abstract void SendObjectUpdate(GameObject obj);
        public abstract void SendLivingDataUpdate(GameLiving living, bool updateStrings); // 코드 작업 안함
        public abstract void SendModelAndSizeChange(ushort objectId, ushort newModel, byte newSize);      
        public abstract void SendStatusUpdate();
        public abstract void SendCombatAnimation(GameObject attacker, GameObject defender, ushort weaponID,
            ushort shieldID, int style, byte stance, byte result, byte targetHealthPercent);
        public abstract void SendSpellCastAnimation(GameLiving spellCaster, ushort spellID, ushort castingTime);
        public abstract void SendSpellEffectAnimation(GameObject spellCaster, GameObject spellTarget, ushort spellid, ushort boltTime, bool noSound, byte success);
        public abstract void SendEmoteAnimation(GameObject obj, eEmote emote);
        public abstract void SendUpdateIcons(System.Collections.IList changedEffects, ref int lastUpdateEffectsCount);
        public abstract void SendStatusUpdate(bool sittingFlag);
        public abstract void SendPlayerQuit(bool totalOut);
        public abstract void SendMessage(string message, eChatType type, eChatLoc loc);
        public abstract void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type, bool autoWrapText, string message);
        public abstract void SendUpdatePlayer();
        public abstract void SendPlayerForgedPosition(GamePlayer obj);
        public abstract void SendObjectRemove(GameObject obj);
        public abstract void SendPlayerCreate(GamePlayer obj);
        public abstract void SendLivingEquipmentUpdate(GameLiving obj);
        public abstract void SendConcentrationList();
        public abstract void SendUpdateMaxSpeed();
        public abstract void SendObjectGuildID(GameObject obj, Guild.Guild guild);
        public abstract void SendCheckLOS(GameObject source, GameObject target, CheckLOSMgrResponse callback);
    }
}
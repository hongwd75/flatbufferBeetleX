using BeetleX;
using Flatbuffers.Messages.Enums;
using Game.Logic.Geometry;
using Game.Logic.World;
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

        public override void SendObjectUpdate(GameObject obj)
        {
            Zone z = obj.CurrentZone;
            
            if (z == null || Client.Player == null || Client.Player.IsVisibleTo(obj) == false)
            {
                return;
            }
            
            var currentZoneCoord = obj.Coordinate - z.Offset;
            var targetZoneCoord = Coordinate.Zero;
            
            int speed = 0;
            ushort targetZone = 0;
            byte flags = 0;
            int targetOID = 0;
            
            if (obj is GameNPC)
            {
                var npc = obj as GameNPC;
                flags = (byte) (GameServer.ServerRules.GetLivingRealm(Client.Player, npc) << 6);

                if (Client.Account.PrivLevel < 2)
                {
                    // no name only if normal player
                    if (npc.IsCannotTarget)
                        flags |= 0x01;
                    if (npc.IsDontShowName)
                        flags |= 0x02;
                }
                if (npc.IsStatue)
                {
                    flags |= 0x01;
                }
                if (npc.IsUnderwater)
                {
                    flags |= 0x10;
                }
                if (npc.IsFlying)
                {
                    flags |= 0x20;
                }

                if (npc.IsMoving && !npc.IsAtTargetLocation)
                {
                    speed = npc.CurrentSpeed;
                    if (npc.Destination != Coordinate.Nowhere && npc.Destination != npc.Coordinate)
                    {
                        Zone tz = npc.CurrentRegion.GetZone(npc.Destination);
                        if (tz != null)
                        {
                            targetZoneCoord = npc.Destination - tz.Offset;

                            var overshootVector = targetZoneCoord - currentZoneCoord;
                            overshootVector = overshootVector * (100/overshootVector.Length);
                            targetZoneCoord += overshootVector;
                            //Dinberg:Instances - zoneSkinID for object positioning clientside.
                            targetZone = tz.ZoneSkinID;
                        }
                    }

                    if (speed > 0x07FF)
                    {
                        speed = 0x07FF;
                    }
                    else if (speed < 0)
                    {
                        speed = 0;
                    }
                }

                GameObject target = npc.TargetObject;
                if (!npc.IsMoving && target != null && target.ObjectState == GameObject.eObjectState.Active && !npc.IsTurningDisabled)
                    targetOID = (ushort) target.ObjectID;
            }
            
            SC_ObjectUpdate_FBS req = new SC_ObjectUpdate_FBS()
            {
                Heading = obj.Orientation.InHeading,
                Currentzonepos = currentZoneCoord.Vector3int,
                Targetzonepos = targetZoneCoord.Vector3int,
            }
        }
        public override void SendLivingDataUpdate(GameLiving living, bool updateStrings)
        {
            if (living == null)
                return;

            // if (living is GamePlayer)
            // {
            //     SendObjectRemove(living);
            //     SendPlayerCreate(living as GamePlayer);
            //     SendLivingEquipmentUpdate(living as GamePlayer);
            // }
            // else if (living is GameNPC)
            // {
            //     SendNPCCreate(living as GameNPC);
            //     if ((living as GameNPC).Inventory != null)
            //         SendLivingEquipmentUpdate(living as GameNPC);
            // }            
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
        public override void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type, bool autoWrapText, string message)
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
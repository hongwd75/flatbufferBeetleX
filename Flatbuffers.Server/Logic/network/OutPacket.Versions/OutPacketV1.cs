using System.Diagnostics;
using BeetleX;
using Flatbuffers.Messages.Enums;
using Game.Logic.Geometry;
using Game.Logic.PropertyCalc;
using Game.Logic.World;
using Game.Logic.World.Timer;
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
            SC_LoginAns_FBS req = new SC_LoginAns_FBS()
            {
                Errorcode = (int)error
            };
            
            SendFlatBufferPacket(ServerPackets.SC_LoginAns, req);
        }

        public override void SendLoginInfo()
        {
            SC_LoginAns_FBS req = new SC_LoginAns_FBS()
            {
                Errorcode = 0,
                Nickname = Client.Account.Name,
                Sessionid = Client.PlayerArrayID
            };
            
            SendFlatBufferPacket(ServerPackets.SC_LoginAns, req);
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
            };
            
            SendFlatBufferPacket(ServerPackets.SC_ObjectUpdate, req);
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
            SC_Quit_FBS req = new SC_Quit_FBS()
            {
                Totalout = totalOut,
                Level = Client == null ? (byte)0 : Client.Player.Level
            };
            
            SendFlatBufferPacket(ServerPackets.SC_Quit, req);
        }
        public override void SendMessage(string message, eChatType type, eChatLoc loc)
        {
            SC_StringMessage_FBS req = new SC_StringMessage_FBS()
            {
                Seesionid = Client.PlayerArrayID,
                Chatloc = loc,
                Chattype = type,
                Message = message
            };

            SendFlatBufferPacket(ServerPackets.SC_StringMessage, req);
        }
        public override void SendDialogBox(eDialogCode code, ushort data1, ushort data2, ushort data3, ushort data4, eDialogType type, bool autoWrapText, string message)
        {
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
            SendFlatBufferPacket(ServerPackets.SC_DialogBoxMessage, req);
        }

        public override void SendUpdatePlayer()
        {
            SC_VariousUpdate_FBS req = new SC_VariousUpdate_FBS()
            {
                Baseclass = Client.Player.CharacterClass.BaseName,
                Classname = Client.Player.Salutation,
                Maxhealth = Client.Player.MaxHealth,
                Level = Client.Player.Level,
                Lastname = Client.Player.LastName,
                Guildname = Client.Account.GuildID,
                Language = Client.Account.Language
            };
            SendFlatBufferPacket(ServerPackets.SC_VariousUpdate, req);
        }

        public override void SendObjectRemove(GameObject obj)
        {
            if (Client.GameObjectUpdateArray.ContainsKey(new Tuple<ushort, ushort>(obj.CurrentRegionID,
                    (ushort)obj.ObjectID)) == true)
            {
                long dummy;
                Client.GameObjectUpdateArray.TryRemove(new Tuple<ushort, ushort>(obj.CurrentRegionID,
                    (ushort)obj.ObjectID), out dummy);
            }

            ushort type = obj switch
            {
                GamePlayer => 2,
                GameNPC => (ushort)(((GameLiving) obj).IsAlive ? 1 : 0),
                _ => 0
            };

            SC_RemoveObject_FBS req = new SC_RemoveObject_FBS()
            {
                Objectid = (ushort)obj.ObjectID,
                Type = type
            };
            
            SendFlatBufferPacket(ServerPackets.SC_RemoveObject, req);
        }

        public override void SendPlayerCreate(GamePlayer obj)
        {
            byte flags = (byte) ((GameServer.ServerRules.GetLivingRealm(Client.Player, obj) & 0x03) << 2);
            if (obj.IsAlive == false) flags |= 0x01;
            if (obj.IsUnderwater) flags |= 0x02; //swimming
            if (obj.IsStealthed) flags |= 0x10;
            
            SC_PlayerCreate_FBS req = new SC_PlayerCreate_FBS()
            {
                Position = obj.Position.vector3pos,
                Sessionid = obj.Network.PlayerArrayID,
                Objectid = (ushort)obj.ObjectID,
                Heading = obj.Orientation.InHeading,
                Level = obj.Level,
                Model = obj.Model,
                Flags = flags,
                Name = obj.Name,
                Lastname = obj.LastName,
                Guildname = obj.GuildName
            };
            SendFlatBufferPacket(ServerPackets.SC_PlayerCreate, req);

            Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID)] =
                GameTimer.GetTickCount();
        }

        public override void SendLivingEquipmentUpdate(GameLiving obj)
        {
            
        }

        public override void SendConcentrationList()
        {
            SC_ConcentrationList_FBS req = new SC_ConcentrationList_FBS();
            lock (Client.Player.ConcentrationEffects)
            {
                for (int i = 0; i < Client.Player.ConcentrationEffects.Count; i++)
                {
                    var effect = Client.Player.ConcentrationEffects[i];
                    req.Coninfo.Add(new ConEffectData_FBS()
                    {
                        Count = (byte)i,
                        Concentration = effect.Concentration,
                        Icon = effect.Icon,
                        Effectname = effect.Name,
                        Ownername = effect.OwnerName
                    });
                }
            }
            SendFlatBufferPacket(ServerPackets.SC_ConcentrationList, req);
        }

        public override void SendUpdateMaxSpeed()
        {
            SC_MaxSpeed_FBS req = new SC_MaxSpeed_FBS()
            {
                Speed = (ushort)(Client.Player.MaxSpeed * 100 / GamePlayer.PLAYER_BASE_SPEED),
                Turningdisabled = Client.Player.IsTurningDisabled,
                Waterspeed = (byte)Math.Min(byte.MaxValue,
                    (Client.Player.MaxSpeed * 100 / GamePlayer.PLAYER_BASE_SPEED) *
                    (Client.Player.GetModified(eProperty.WaterSpeed)*0.01f))
            };
            SendFlatBufferPacket(ServerPackets.SC_MaxSpeed, req);
        }
    }
}
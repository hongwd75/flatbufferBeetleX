using System.Diagnostics;
using BeetleX;
using Flatbuffers.Messages.Enums;
using Game.Logic.AI.Brain;
using Game.Logic.datatable;
using Game.Logic.Geometry;
using Game.Logic.Language;
using Game.Logic.PropertyCalc;
using Game.Logic.Utils;
using Game.Logic.World;
using Game.Logic.World.Timer;
using Google.FlatBuffers;
using Logic.database.table;
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

        public override void SendModelAndSizeChange(ushort objectId, ushort newModel, byte newSize)
        {
            SC_ModelChange_FBS req = new SC_ModelChange_FBS()
            {
                Objectid = objectId,
                Model = newModel,
                Newsize = newSize
            };
            
            SendFlatBufferPacket(ServerPackets.SC_ModelChange, req);
        }
        
        public override void SendMovingObjectCreate(GameMovingObject obj)
        {
            string name = obj.Name;

            LanguageDataObject translation = LanguageMgr.GetTranslation(Client, obj);
            if (translation != null)
            {
                if (!Util.IsEmpty(((DBLanguageGameObject)translation).Name))
                {
                    name = ((DBLanguageGameObject)translation).Name;
                }
            }            
            
            SC_MovingObjectCreate_FBS req = new SC_MovingObjectCreate_FBS()
            {
                Objectid = (ushort)obj.ObjectID,
                Heading = obj.Orientation.InHeading,
                Model = obj.Model,
                Position = obj.Position.vector3pos,
                Name = name,
                Flags = (obj.Type() | ((byte)obj.Realm == 3 ? 0x40 : (byte)obj.Realm << 4) | obj.GetDisplayLevel(Client.Player) << 9),
                Emblem = obj.Emblem
            };
            
            SendFlatBufferPacket(ServerPackets.SC_MovingObjectCreate, req);
            
            // Update Cache
            Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(obj.CurrentRegionID, (ushort)obj.ObjectID)] = GameTimer.GetTickCount();            
        }
        
        public override void SendObjectCreate(GameObject obj)
        {
            if (obj == null || obj.IsVisibleTo(Client.Player) == false)
            {
                return;
            }

            string name = obj.Name;
            int emblem = 0;
            ushort model = obj.Model;
            int flag = ((byte) obj.Realm & 3) << 4;
            if (obj.IsUnderwater)
            {
                if (obj is GameNPC) model |= 0x8000;
                else flag |= 0x01;
            }
            if (obj is GameStaticItemTimed && Client.Player != null &&
                (obj as GameStaticItemTimed).IsOwner(Client.Player))
                flag |= 0x04;
            
            if (obj is GameStaticItem staicitem)
            {
                emblem = staicitem.Emblem;
                LanguageDataObject translation = LanguageMgr.GetTranslation(Client, staicitem);
                if (translation != null)
                {
                    if (obj is WorldInventoryItem)
                    {
                        
                    } else
                    if (!Util.IsEmpty(((DBLanguageGameObject)translation).Name))
                    {
                        name = ((DBLanguageGameObject)translation).Name;
                    }
                }
            }

            SC_ObjectCreate_FBS req = new SC_ObjectCreate_FBS()
            {
                Objectid = (ushort)obj.ObjectID,
                Heading = obj.Orientation.InHeading,
                Position = obj.Position.vector3pos,
                Model = model,                
                Name = name,
                Emblem = (ushort)emblem,
                Flags = flag
            };
        }
        
        public override void SendNPCCreate(GameNPC npc)
        {
            if (npc == null || Client.Player == null || npc.IsVisibleTo(Client.Player) == false)
            {
                return;
            }
            
            if (npc is GameMovingObject)
            {
                SendMovingObjectCreate(npc as GameMovingObject);
                return;
            }
            
            int speed = 0;
            ushort speedz = 0;

            if (npc.IsAtTargetLocation == false)
            {
                speed = npc.CurrentSpeed;
                speedz = (ushort)npc.ZSpeedFactor;
            }

            byte flags = 0;
            SetFlag(ref flags, (GameServer.ServerRules.GetLivingRealm(Client.Player, npc) > 0), (byte)(1 << 6));
            SetFlag(ref flags, (npc.Flags & GameNPC.eFlags.GHOST) != 0, 0x01);
            SetFlag(ref flags, npc.Inventory != null, 0x02);
            SetFlag(ref flags, npc.IsPeaceful, 0x10);
            SetFlag(ref flags, npc.IsFlying, 0x20);
            SetFlag(ref flags, npc.IsTorchLit, 0x04);

            byte flags2 = 0;
            SetFlag(ref flags2, npc.Brain is IControlledBrain, 0x80);
            SetFlag(ref flags2, npc.IsCannotTarget, 0x01);
            SetFlag(ref flags2, npc.IsDontShowName, 0x02);
            SetFlag(ref flags2, npc.IsDontShowName, 0x04);

            byte flags3 = 0; // 나중에 퀘스트 용
            
            SC_NPCCreate_FBS req = new SC_NPCCreate_FBS()
            {
                Objectid = (ushort)npc.ObjectID,
                Heading = npc.Orientation.InHeading,
                Model = npc.Model,
                Position = npc.Position.vector3pos,
                Name = npc.Name,
                Guildname = npc.GuildName,
                Level = npc.Level,
                Speed = (ushort)speed,
                Speedz = speedz,
                Flags = flags,
                Flags2 = flags2,
                Flags3 = flags3
                
            };
            SendFlatBufferPacket(ServerPackets.SC_NPCCreate, req);
        }

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

        public override void SendPlayerForgedPosition(GamePlayer obj)
        {
            // 아무것도 안해도 됨
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

        public override void SendCombatAnimation(GameObject attacker, GameObject defender, ushort weaponID,
            ushort shieldID, int style, byte stance, byte result, byte targetHealthPercent)
        {
            SC_CombatAnimation_FBS req = new SC_CombatAnimation_FBS()
            {
                Attackobjectid = (ushort)(attacker != null ? attacker.ObjectID : 0),
                Defenderobjectid = (ushort)(defender != null ? defender.ObjectID : 0),
                Healthpercent = targetHealthPercent,
                Weaponid = weaponID,
                Shieldid = shieldID,
                Stance = stance,
                Style = (ushort)style,
                Result = result
            };
            SendFlatBufferPacket(ServerPackets.SC_CombatAnimation, req);
        }

        public override void SendSpellCastAnimation(GameLiving spellCaster, ushort spellID, ushort castingTime)
        {
            SC_SpellCastAnimation_FBS req = new SC_SpellCastAnimation_FBS()
            {
                Objectid = (ushort)spellCaster.ObjectID,
                Spellid = spellID,
                Castingtime = castingTime
            };
            SendFlatBufferPacket(ServerPackets.SC_SpellCastAnimation, req);
        }

        public override void SendSpellEffectAnimation(GameObject spellCaster, GameObject spellTarget, ushort spellid, ushort boltTime, bool noSound, byte success)
        {
            SC_SpellEffectAnimation_FBS req = new SC_SpellEffectAnimation_FBS()
            {
                Casterobjectid = (ushort)spellCaster.ObjectID,
                Spellid = spellid,
                Spelltarget = spellTarget == null ? (ushort)0 : (ushort)spellTarget.ObjectID,
                Bolttime = boltTime,
                Nosound = noSound,
                Success = (bool) (success != 0)
            };
            SendFlatBufferPacket(ServerPackets.SC_SpellEffectAnimation, req);            
        }
        public override void SendEmoteAnimation(GameObject obj, eEmote emote)
        {
            SC_EmoteAnimation_FBS req = new SC_EmoteAnimation_FBS()
            {
                Objectid = (ushort)obj.ObjectID,
                Emote = (byte)emote
            };
            SendFlatBufferPacket(ServerPackets.SC_EmoteAnimation, req);
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

        public override void SendStatusUpdate()
        {
            SendStatusUpdate(Player.IsSitting);
        }
        public override void SendStatusUpdate(bool sittingFlag)
        {
            SC_CharacterStatusUpdate_FBS req = new SC_CharacterStatusUpdate_FBS()
            {
                Sittingflag = sittingFlag,
                Healthpercent = Player.HealthPercent,
                Manapercent = Player.ManaPercent,
                Endurancepercent = Player.EndurancePercent,
                Conpercent = Player.ConcentrationPercent,
                Maxmana = Player.MaxMana,
                Maxhealth = Player.MaxHealth,
                Maxendurance = Player.MaxEndurance,
                Maxconcetration = Player.MaxConcentration,
                Mana = Player.Mana,
                Concentration = Player.Concentration,
                Endurance = Player.Endurance,
                Health = Player.Health
            };
            SendFlatBufferPacket(ServerPackets.SC_CharacterStatusUpdate, req);
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
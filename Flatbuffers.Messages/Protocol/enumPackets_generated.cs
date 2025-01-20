// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace NetworkMessage
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public enum eChatType : byte
{
  CT_System = 0,
  CT_Say = 1,
  CT_Send = 2,
  CT_Chat = 3,
  CT_Guild = 4,
  CT_Merchant = 5,
  CT_Spell = 6,
  CT_SpellExpires = 7,
  CT_SpellResisted = 8,
  CT_YouHit = 9,
  CT_Loot = 10,
  CT_Broadcast = 11,
  CT_Help = 12,
  CT_Staff = 13,
  CT_PlayerDied = 14,
  CT_KilledByEnemy = 15,
  CT_YouDied = 16,
  CT_Important = 17,
};

public enum eChatLoc : byte
{
  CL_ChatWindow = 0,
  CL_PopupWindow = 1,
  CL_SystemWindow = 2,
};

public enum eDialogType : byte
{
  Ok = 0,
  Warmap = 1,
  YesNo = 1,
};

public enum eDialogCode : byte
{
  SimpleWarning = 0,
  GuildInvite = 3,
  GroupInvite = 5,
  CustomDialog = 6,
  GuildLeave = 8,
};

public enum ServerPackets : ushort
{
  SC_LoginAns = 1,
  SC_AccountInfo = 2,
  SC_VariousUpdate = 3,
  SC_ObjectUpdate = 4,
  SC_CharacterStatusUpdate = 5,
  SC_PlayerCreate = 6,
  SC_MovingObjectCreate = 7,
  SC_NPCCreate = 8,
  SC_ObjectCreate = 9,
  SC_ModelChange = 10,
  SC_RemoveObject = 11,
  SC_ConcentrationList = 12,
  SC_CombatAnimation = 13,
  SC_SpellCastAnimation = 14,
  SC_SpellEffectAnimation = 15,
  SC_EmoteAnimation = 16,
  SC_StringMessage = 17,
  SC_DialogBoxMessage = 18,
  SC_MaxSpeed = 19,
  SC_Quit = 20,
};

public enum ClientPackets : ushort
{
  CS_LoginReq = 1,
  CS_WorldJoinReq = 2,
  CS_UpdatePosition = 3,
};

public enum eEmote : byte
{
  Beckon = 1,
  Blush = 2,
  Bow = 3,
  Cheer = 4,
  Clap = 5,
  Cry = 6,
  Curtsey = 7,
  Flex = 8,
  BlowKiss = 9,
  Dance = 10,
  Laugh = 11,
  Point = 12,
  Salute = 13,
  BangOnShield = 14,
  Victory = 15,
  Wave = 16,
  Distract = 17,
  MidgardFrenzy = 18,
  ThrowDirt = 19,
  StagFrenzy = 20,
  Roar = 21,
  Drink = 22,
  Ponder = 23,
  Military = 24,
  Present = 25,
  Rude = 27,
  Taunt = 28,
  Hug = 29,
  LetsGo = 30,
  Meditate = 31,
  No = 32,
  Raise = 33,
  Shrug = 34,
  Slap = 35,
  Slit = 36,
  Surrender = 37,
  Yes = 38,
  Beg = 39,
  Induct = 40,
  Dismiss = 41,
  LvlUp = 42,
  Pray = 43,
  Bind = 44,
  SpellGoBoom = 45,
  Knock = 46,
  Smile = 47,
  Angry = 48,
  Rider_LookFar = 49,
  Rider_Stench = 50,
  Rider_Halt = 51,
  Rider_pet = 52,
  Horse_Courbette = 53,
  Horse_Startle = 54,
  Horse_Nod = 55,
  Horse_Graze = 56,
  Horse_rear = 57,
  Sweat = 58,
  Stagger = 59,
  Rider_Trick = 60,
  Yawn = 61,
  Doh = 62,
  Confused = 63,
  Shiver = 64,
  Rofl = 65,
  Mememe = 66,
  Horse_whistle = 67,
  Worship = 68,
  PlayerPrepare = 69,
  PlayerPickup = 70,
  PlayerListen = 71,
  BindAlb = 73,
  BindMid = 74,
  BindHib = 75,
  Howl = 76,
  Diabolical = 77,
  Brandish = 79,
  Startled = 80,
  Talk = 81,
  Monty = 84,
  Loco = 85,
  Cower = 91,
  SiegeWeaponEmote = 201,
};


}

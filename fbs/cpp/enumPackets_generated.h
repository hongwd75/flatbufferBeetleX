// automatically generated by the FlatBuffers compiler, do not modify


#ifndef FLATBUFFERS_GENERATED_ENUMPACKETS_NETWORKMESSAGE_H_
#define FLATBUFFERS_GENERATED_ENUMPACKETS_NETWORKMESSAGE_H_

#include "flatbuffers/flatbuffers.h"

// Ensure the included flatbuffers.h is the same version as when this file was
// generated, otherwise it may not be compatible.
static_assert(FLATBUFFERS_VERSION_MAJOR == 24 &&
              FLATBUFFERS_VERSION_MINOR == 3 &&
              FLATBUFFERS_VERSION_REVISION == 25,
             "Non-compatible flatbuffers version included");


namespace NetworkMessage {

enum eChatType : int8_t {
  eChatType_CT_System = 0,
  eChatType_CT_Say = 1,
  eChatType_CT_Send = 2,
  eChatType_CT_Chat = 3,
  eChatType_CT_Guild = 4,
  eChatType_CT_Merchant = 5,
  eChatType_CT_Loot = 6,
  eChatType_CT_Broadcast = 7,
  eChatType_CT_Help = 8,
  eChatType_CT_Staff = 9,
  eChatType_CT_Important = 10,
  eChatType_MIN = eChatType_CT_System,
  eChatType_MAX = eChatType_CT_Important
};

inline const eChatType (&EnumValueseChatType())[11] {
  static const eChatType values[] = {
    eChatType_CT_System,
    eChatType_CT_Say,
    eChatType_CT_Send,
    eChatType_CT_Chat,
    eChatType_CT_Guild,
    eChatType_CT_Merchant,
    eChatType_CT_Loot,
    eChatType_CT_Broadcast,
    eChatType_CT_Help,
    eChatType_CT_Staff,
    eChatType_CT_Important
  };
  return values;
}

inline const char * const *EnumNameseChatType() {
  static const char * const names[12] = {
    "CT_System",
    "CT_Say",
    "CT_Send",
    "CT_Chat",
    "CT_Guild",
    "CT_Merchant",
    "CT_Loot",
    "CT_Broadcast",
    "CT_Help",
    "CT_Staff",
    "CT_Important",
    nullptr
  };
  return names;
}

inline const char *EnumNameeChatType(eChatType e) {
  if (::flatbuffers::IsOutRange(e, eChatType_CT_System, eChatType_CT_Important)) return "";
  const size_t index = static_cast<size_t>(e);
  return EnumNameseChatType()[index];
}

enum eChatLoc : int8_t {
  eChatLoc_CL_ChatWindow = 0,
  eChatLoc_CL_PopupWindow = 1,
  eChatLoc_CL_SystemWindow = 2,
  eChatLoc_MIN = eChatLoc_CL_ChatWindow,
  eChatLoc_MAX = eChatLoc_CL_SystemWindow
};

inline const eChatLoc (&EnumValueseChatLoc())[3] {
  static const eChatLoc values[] = {
    eChatLoc_CL_ChatWindow,
    eChatLoc_CL_PopupWindow,
    eChatLoc_CL_SystemWindow
  };
  return values;
}

inline const char * const *EnumNameseChatLoc() {
  static const char * const names[4] = {
    "CL_ChatWindow",
    "CL_PopupWindow",
    "CL_SystemWindow",
    nullptr
  };
  return names;
}

inline const char *EnumNameeChatLoc(eChatLoc e) {
  if (::flatbuffers::IsOutRange(e, eChatLoc_CL_ChatWindow, eChatLoc_CL_SystemWindow)) return "";
  const size_t index = static_cast<size_t>(e);
  return EnumNameseChatLoc()[index];
}

enum eDialogType : int8_t {
  eDialogType_Ok = 0,
  eDialogType_Warmap = 1,
  eDialogType_YesNo = 1,
  eDialogType_MIN = eDialogType_Ok,
  eDialogType_MAX = eDialogType_YesNo
};

inline const eDialogType (&EnumValueseDialogType())[3] {
  static const eDialogType values[] = {
    eDialogType_Ok,
    eDialogType_Warmap,
    eDialogType_YesNo
  };
  return values;
}

inline const char * const *EnumNameseDialogType() {
  static const char * const names[3] = {
    "Ok",
    "Warmap",
    "YesNo",
    nullptr
  };
  return names;
}

inline const char *EnumNameeDialogType(eDialogType e) {
  if (::flatbuffers::IsOutRange(e, eDialogType_Ok, eDialogType_YesNo)) return "";
  const size_t index = static_cast<size_t>(e);
  return EnumNameseDialogType()[index];
}

enum eDialogCode : int8_t {
  eDialogCode_SimpleWarning = 0,
  eDialogCode_GuildInvite = 3,
  eDialogCode_GroupInvite = 5,
  eDialogCode_CustomDialog = 6,
  eDialogCode_GuildLeave = 8,
  eDialogCode_MIN = eDialogCode_SimpleWarning,
  eDialogCode_MAX = eDialogCode_GuildLeave
};

inline const eDialogCode (&EnumValueseDialogCode())[5] {
  static const eDialogCode values[] = {
    eDialogCode_SimpleWarning,
    eDialogCode_GuildInvite,
    eDialogCode_GroupInvite,
    eDialogCode_CustomDialog,
    eDialogCode_GuildLeave
  };
  return values;
}

inline const char * const *EnumNameseDialogCode() {
  static const char * const names[10] = {
    "SimpleWarning",
    "",
    "",
    "GuildInvite",
    "",
    "GroupInvite",
    "CustomDialog",
    "",
    "GuildLeave",
    nullptr
  };
  return names;
}

inline const char *EnumNameeDialogCode(eDialogCode e) {
  if (::flatbuffers::IsOutRange(e, eDialogCode_SimpleWarning, eDialogCode_GuildLeave)) return "";
  const size_t index = static_cast<size_t>(e);
  return EnumNameseDialogCode()[index];
}

enum ServerPackets : uint16_t {
  ServerPackets_SC_LoginAns = 1,
  ServerPackets_SC_AccountInfo = 2,
  ServerPackets_SC_VariousUpdate = 3,
  ServerPackets_SC_ObjectUpdate = 4,
  ServerPackets_SC_PlayerCreate = 5,
  ServerPackets_SC_RemoveObject = 6,
  ServerPackets_SC_ConcentrationList = 7,
  ServerPackets_SC_StringMessage = 8,
  ServerPackets_SC_DialogBoxMessage = 9,
  ServerPackets_SC_Quit = 10,
  ServerPackets_MIN = ServerPackets_SC_LoginAns,
  ServerPackets_MAX = ServerPackets_SC_Quit
};

inline const ServerPackets (&EnumValuesServerPackets())[10] {
  static const ServerPackets values[] = {
    ServerPackets_SC_LoginAns,
    ServerPackets_SC_AccountInfo,
    ServerPackets_SC_VariousUpdate,
    ServerPackets_SC_ObjectUpdate,
    ServerPackets_SC_PlayerCreate,
    ServerPackets_SC_RemoveObject,
    ServerPackets_SC_ConcentrationList,
    ServerPackets_SC_StringMessage,
    ServerPackets_SC_DialogBoxMessage,
    ServerPackets_SC_Quit
  };
  return values;
}

inline const char * const *EnumNamesServerPackets() {
  static const char * const names[11] = {
    "SC_LoginAns",
    "SC_AccountInfo",
    "SC_VariousUpdate",
    "SC_ObjectUpdate",
    "SC_PlayerCreate",
    "SC_RemoveObject",
    "SC_ConcentrationList",
    "SC_StringMessage",
    "SC_DialogBoxMessage",
    "SC_Quit",
    nullptr
  };
  return names;
}

inline const char *EnumNameServerPackets(ServerPackets e) {
  if (::flatbuffers::IsOutRange(e, ServerPackets_SC_LoginAns, ServerPackets_SC_Quit)) return "";
  const size_t index = static_cast<size_t>(e) - static_cast<size_t>(ServerPackets_SC_LoginAns);
  return EnumNamesServerPackets()[index];
}

enum ClientPackets : uint16_t {
  ClientPackets_CS_LoginReq = 1,
  ClientPackets_CS_WorldJoinReq = 2,
  ClientPackets_CS_UpdatePosition = 3,
  ClientPackets_MIN = ClientPackets_CS_LoginReq,
  ClientPackets_MAX = ClientPackets_CS_UpdatePosition
};

inline const ClientPackets (&EnumValuesClientPackets())[3] {
  static const ClientPackets values[] = {
    ClientPackets_CS_LoginReq,
    ClientPackets_CS_WorldJoinReq,
    ClientPackets_CS_UpdatePosition
  };
  return values;
}

inline const char * const *EnumNamesClientPackets() {
  static const char * const names[4] = {
    "CS_LoginReq",
    "CS_WorldJoinReq",
    "CS_UpdatePosition",
    nullptr
  };
  return names;
}

inline const char *EnumNameClientPackets(ClientPackets e) {
  if (::flatbuffers::IsOutRange(e, ClientPackets_CS_LoginReq, ClientPackets_CS_UpdatePosition)) return "";
  const size_t index = static_cast<size_t>(e) - static_cast<size_t>(ClientPackets_CS_LoginReq);
  return EnumNamesClientPackets()[index];
}

}  // namespace NetworkMessage

#endif  // FLATBUFFERS_GENERATED_ENUMPACKETS_NETWORKMESSAGE_H_

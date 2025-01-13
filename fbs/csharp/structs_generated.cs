// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace NetworkMessage
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct ConEffectData : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_24_3_25(); }
  public static ConEffectData GetRootAsConEffectData(ByteBuffer _bb) { return GetRootAsConEffectData(_bb, new ConEffectData()); }
  public static ConEffectData GetRootAsConEffectData(ByteBuffer _bb, ConEffectData obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public ConEffectData __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public byte Count { get { int o = __p.__offset(4); return o != 0 ? __p.bb.Get(o + __p.bb_pos) : (byte)0; } }
  public byte Concentration { get { int o = __p.__offset(6); return o != 0 ? __p.bb.Get(o + __p.bb_pos) : (byte)0; } }
  public ushort Icon { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetUshort(o + __p.bb_pos) : (ushort)0; } }
  public string Effectname { get { int o = __p.__offset(10); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetEffectnameBytes() { return __p.__vector_as_span<byte>(10, 1); }
#else
  public ArraySegment<byte>? GetEffectnameBytes() { return __p.__vector_as_arraysegment(10); }
#endif
  public byte[] GetEffectnameArray() { return __p.__vector_as_array<byte>(10); }
  public string Ownername { get { int o = __p.__offset(12); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetOwnernameBytes() { return __p.__vector_as_span<byte>(12, 1); }
#else
  public ArraySegment<byte>? GetOwnernameBytes() { return __p.__vector_as_arraysegment(12); }
#endif
  public byte[] GetOwnernameArray() { return __p.__vector_as_array<byte>(12); }

  public static Offset<NetworkMessage.ConEffectData> CreateConEffectData(FlatBufferBuilder builder,
      byte count = 0,
      byte concentration = 0,
      ushort icon = 0,
      StringOffset effectnameOffset = default(StringOffset),
      StringOffset ownernameOffset = default(StringOffset)) {
    builder.StartTable(5);
    ConEffectData.AddOwnername(builder, ownernameOffset);
    ConEffectData.AddEffectname(builder, effectnameOffset);
    ConEffectData.AddIcon(builder, icon);
    ConEffectData.AddConcentration(builder, concentration);
    ConEffectData.AddCount(builder, count);
    return ConEffectData.EndConEffectData(builder);
  }

  public static void StartConEffectData(FlatBufferBuilder builder) { builder.StartTable(5); }
  public static void AddCount(FlatBufferBuilder builder, byte count) { builder.AddByte(0, count, 0); }
  public static void AddConcentration(FlatBufferBuilder builder, byte concentration) { builder.AddByte(1, concentration, 0); }
  public static void AddIcon(FlatBufferBuilder builder, ushort icon) { builder.AddUshort(2, icon, 0); }
  public static void AddEffectname(FlatBufferBuilder builder, StringOffset effectnameOffset) { builder.AddOffset(3, effectnameOffset.Value, 0); }
  public static void AddOwnername(FlatBufferBuilder builder, StringOffset ownernameOffset) { builder.AddOffset(4, ownernameOffset.Value, 0); }
  public static Offset<NetworkMessage.ConEffectData> EndConEffectData(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<NetworkMessage.ConEffectData>(o);
  }
  public ConEffectData_FBS UnPack() {
    var _o = new ConEffectData_FBS();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(ConEffectData_FBS _o) {
    _o.Count = this.Count;
    _o.Concentration = this.Concentration;
    _o.Icon = this.Icon;
    _o.Effectname = this.Effectname;
    _o.Ownername = this.Ownername;
  }
  public static Offset<NetworkMessage.ConEffectData> Pack(FlatBufferBuilder builder, ConEffectData_FBS _o) {
    if (_o == null) return default(Offset<NetworkMessage.ConEffectData>);
    var _effectname = _o.Effectname == null ? default(StringOffset) : builder.CreateString(_o.Effectname);
    var _ownername = _o.Ownername == null ? default(StringOffset) : builder.CreateString(_o.Ownername);
    return CreateConEffectData(
      builder,
      _o.Count,
      _o.Concentration,
      _o.Icon,
      _effectname,
      _ownername);
  }
}

public class ConEffectData_FBS
{
  public byte Count { get; set; }
  public byte Concentration { get; set; }
  public ushort Icon { get; set; }
  public string Effectname { get; set; }
  public string Ownername { get; set; }

  public ConEffectData_FBS() {
    this.Count = 0;
    this.Concentration = 0;
    this.Icon = 0;
    this.Effectname = null;
    this.Ownername = null;
  }
}


static public class ConEffectDataVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Count*/, 1 /*byte*/, 1, false)
      && verifier.VerifyField(tablePos, 6 /*Concentration*/, 1 /*byte*/, 1, false)
      && verifier.VerifyField(tablePos, 8 /*Icon*/, 2 /*ushort*/, 2, false)
      && verifier.VerifyString(tablePos, 10 /*Effectname*/, false)
      && verifier.VerifyString(tablePos, 12 /*Ownername*/, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}
public struct Vector3 : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public Vector3 __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public float X { get { return __p.bb.GetFloat(__p.bb_pos + 0); } }
  public float Y { get { return __p.bb.GetFloat(__p.bb_pos + 4); } }
  public float Z { get { return __p.bb.GetFloat(__p.bb_pos + 8); } }

  public static Offset<NetworkMessage.Vector3> CreateVector3(FlatBufferBuilder builder, float X, float Y, float Z) {
    builder.Prep(4, 12);
    builder.PutFloat(Z);
    builder.PutFloat(Y);
    builder.PutFloat(X);
    return new Offset<NetworkMessage.Vector3>(builder.Offset);
  }
  public Vector3_FBS UnPack() {
    var _o = new Vector3_FBS();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(Vector3_FBS _o) {
    _o.X = this.X;
    _o.Y = this.Y;
    _o.Z = this.Z;
  }
  public static Offset<NetworkMessage.Vector3> Pack(FlatBufferBuilder builder, Vector3_FBS _o) {
    if (_o == null) return default(Offset<NetworkMessage.Vector3>);
    return CreateVector3(
      builder,
      _o.X,
      _o.Y,
      _o.Z);
  }
}

public class Vector3_FBS
{
  public float X { get; set; }
  public float Y { get; set; }
  public float Z { get; set; }

  public Vector3_FBS() {
    this.X = 0.0f;
    this.Y = 0.0f;
    this.Z = 0.0f;
  }
}

public struct Vector3Int : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public Vector3Int __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int X { get { return __p.bb.GetInt(__p.bb_pos + 0); } }
  public int Y { get { return __p.bb.GetInt(__p.bb_pos + 4); } }
  public int Z { get { return __p.bb.GetInt(__p.bb_pos + 8); } }

  public static Offset<NetworkMessage.Vector3Int> CreateVector3Int(FlatBufferBuilder builder, int X, int Y, int Z) {
    builder.Prep(4, 12);
    builder.PutInt(Z);
    builder.PutInt(Y);
    builder.PutInt(X);
    return new Offset<NetworkMessage.Vector3Int>(builder.Offset);
  }
  public Vector3Int_FBS UnPack() {
    var _o = new Vector3Int_FBS();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(Vector3Int_FBS _o) {
    _o.X = this.X;
    _o.Y = this.Y;
    _o.Z = this.Z;
  }
  public static Offset<NetworkMessage.Vector3Int> Pack(FlatBufferBuilder builder, Vector3Int_FBS _o) {
    if (_o == null) return default(Offset<NetworkMessage.Vector3Int>);
    return CreateVector3Int(
      builder,
      _o.X,
      _o.Y,
      _o.Z);
  }
}

public class Vector3Int_FBS
{
  public int X { get; set; }
  public int Y { get; set; }
  public int Z { get; set; }

  public Vector3Int_FBS() {
    this.X = 0;
    this.Y = 0;
    this.Z = 0;
  }
}

public struct CreatePlayerInfo : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_24_3_25(); }
  public static CreatePlayerInfo GetRootAsCreatePlayerInfo(ByteBuffer _bb) { return GetRootAsCreatePlayerInfo(_bb, new CreatePlayerInfo()); }
  public static CreatePlayerInfo GetRootAsCreatePlayerInfo(ByteBuffer _bb, CreatePlayerInfo obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public CreatePlayerInfo __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Sessionid { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public string Name { get { int o = __p.__offset(6); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameBytes() { return __p.__vector_as_span<byte>(6, 1); }
#else
  public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(6); }
#endif
  public byte[] GetNameArray() { return __p.__vector_as_array<byte>(6); }
  public int Realm { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Head { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public NetworkMessage.Vector3? Position { get { int o = __p.__offset(12); return o != 0 ? (NetworkMessage.Vector3?)(new NetworkMessage.Vector3()).__assign(o + __p.bb_pos, __p.bb) : null; } }

  public static Offset<NetworkMessage.CreatePlayerInfo> CreateCreatePlayerInfo(FlatBufferBuilder builder,
      int sessionid = 0,
      StringOffset nameOffset = default(StringOffset),
      int realm = 0,
      int head = 0,
      NetworkMessage.Vector3_FBS position = null) {
    builder.StartTable(5);
    CreatePlayerInfo.AddPosition(builder, NetworkMessage.Vector3.Pack(builder, position));
    CreatePlayerInfo.AddHead(builder, head);
    CreatePlayerInfo.AddRealm(builder, realm);
    CreatePlayerInfo.AddName(builder, nameOffset);
    CreatePlayerInfo.AddSessionid(builder, sessionid);
    return CreatePlayerInfo.EndCreatePlayerInfo(builder);
  }

  public static void StartCreatePlayerInfo(FlatBufferBuilder builder) { builder.StartTable(5); }
  public static void AddSessionid(FlatBufferBuilder builder, int sessionid) { builder.AddInt(0, sessionid, 0); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(1, nameOffset.Value, 0); }
  public static void AddRealm(FlatBufferBuilder builder, int realm) { builder.AddInt(2, realm, 0); }
  public static void AddHead(FlatBufferBuilder builder, int head) { builder.AddInt(3, head, 0); }
  public static void AddPosition(FlatBufferBuilder builder, Offset<NetworkMessage.Vector3> positionOffset) { builder.AddStruct(4, positionOffset.Value, 0); }
  public static Offset<NetworkMessage.CreatePlayerInfo> EndCreatePlayerInfo(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<NetworkMessage.CreatePlayerInfo>(o);
  }
  public CreatePlayerInfo_FBS UnPack() {
    var _o = new CreatePlayerInfo_FBS();
    this.UnPackTo(_o);
    return _o;
  }
  public void UnPackTo(CreatePlayerInfo_FBS _o) {
    _o.Sessionid = this.Sessionid;
    _o.Name = this.Name;
    _o.Realm = this.Realm;
    _o.Head = this.Head;
    _o.Position = this.Position.HasValue ? this.Position.Value.UnPack() : null;
  }
  public static Offset<NetworkMessage.CreatePlayerInfo> Pack(FlatBufferBuilder builder, CreatePlayerInfo_FBS _o) {
    if (_o == null) return default(Offset<NetworkMessage.CreatePlayerInfo>);
    var _name = _o.Name == null ? default(StringOffset) : builder.CreateString(_o.Name);
    return CreateCreatePlayerInfo(
      builder,
      _o.Sessionid,
      _name,
      _o.Realm,
      _o.Head,
      _o.Position);
  }
}

public class CreatePlayerInfo_FBS
{
  public int Sessionid { get; set; }
  public string Name { get; set; }
  public int Realm { get; set; }
  public int Head { get; set; }
  public NetworkMessage.Vector3_FBS Position { get; set; }

  public CreatePlayerInfo_FBS() {
    this.Sessionid = 0;
    this.Name = null;
    this.Realm = 0;
    this.Head = 0;
    this.Position = new NetworkMessage.Vector3_FBS();
  }
}


static public class CreatePlayerInfoVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Sessionid*/, 4 /*int*/, 4, false)
      && verifier.VerifyString(tablePos, 6 /*Name*/, false)
      && verifier.VerifyField(tablePos, 8 /*Realm*/, 4 /*int*/, 4, false)
      && verifier.VerifyField(tablePos, 10 /*Head*/, 4 /*int*/, 4, false)
      && verifier.VerifyField(tablePos, 12 /*Position*/, 12 /*NetworkMessage.Vector3*/, 4, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}

using System.Collections.Concurrent;
using System.Reflection;
using BeetleX.Buffers;
using BeetleX.Clients;
using BeetleX.Packets;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Flatbuffers.Messages.Packets.Client
{

    public class ClientTypeHandler : TypeHandler<ServerPackets, ClientPackets>
    {
        private ConcurrentDictionary<ServerPackets, Func<ByteBuffer, Task>> _packetMessages = new();
        public ConcurrentDictionary<ServerPackets, Func<ByteBuffer, Task>> PacketMsg
        {
            get => _packetMessages;
        }
        
        public ClientTypeHandler(PacketMessageAttribute.PacketType ptype) : base(ptype) { }
        protected override Type GetReadType(ServerPackets id) => typeof((ushort ID, ByteBuffer buffer));

        public override void Register(params Assembly[] assemblies)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == false) continue;
                if (type.GetInterface("Network.Protocol.IPacketMessage") == null) continue;

                var packetattributes =
                    (ServerPacketMessageAttribute[])type.GetCustomAttributes(typeof(ServerPacketMessageAttribute),
                        true);
                if (packetattributes.Length > 0)
                {
                    var handle = (IClientPacketMessage)Activator.CreateInstance(type);
                    _packetMessages.TryAdd((ServerPackets)(object)packetattributes[0].codeid,
                        async (data) => { await handle.Packet(data); });
                }
            }
        }


    }
    
    public class ClientPacket : FixeHeaderClientPacket
    {
        public IMessageTypeHeader TypeHeader { get; set; } = new ClientTypeHandler(PacketMessageAttribute.PacketType.ClientReceiveType);

        public override IClientPacket Clone()
        {
            ClientPacket result = new ClientPacket();
            result.TypeHeader = TypeHeader;
            return result;
        }

        public void Register(params Assembly[] assemblies)
        {
            TypeHeader.Register(assemblies);
        }
        
        protected override object OnRead(IClient client, PipeStream reader)
        {
            // ushort ID 읽기
            ushort id = reader.ReadUInt16();

            // 남은 데이터의 크기 확인
            int size = (int)reader.Length;

            // 바이트 배열 생성 및 데이터 읽기
            byte[] buffer = new byte[size];
            reader.Read(buffer, 0, size);

            // FlatBuffers의 ByteBuffer 생성
            var byteBuffer = new ByteBuffer(buffer);

            return (ID: id, Buffer: byteBuffer);
        }

        protected override void OnWrite(object data, IClient client, PipeStream writer)
        {
            if (data is (ushort ID, byte[] Buffer))
            {
                // ID 쓰기
                writer.Write(ID);
                writer.Write(Buffer, 0, Buffer.Length);
            }
            else
            {
                throw new ArgumentException("데이터는 (ushort, ByteBuffer) 형태여야 합니다.");
            }
        }

        public void OnRecieveData(ServerPackets msg, ByteBuffer buffer)
        {
            ClientTypeHandler handler = (ClientTypeHandler)TypeHeader;
            if (handler.PacketMsg.TryGetValue(msg, out var fnc) == true)
            {
                fnc.Invoke(buffer);
            }
        }
    }      
}
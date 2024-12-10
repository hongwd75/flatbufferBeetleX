using System.Collections.Concurrent;
using System.Reflection;
using BeetleX.Buffers;
using BeetleX.Clients;
using BeetleX.Packets;
using Google.FlatBuffers;
using Network.Protocol;
using NetworkMessage;

namespace Flatbuffers.Messages.Packets.Client
{

    public class ClientTypeHandler : TypeHandler<ServerPackets, ClientPackets>
    {
        private ConcurrentDictionary<ServerPackets, Func<ByteBuffer, Task>> _packetMessages = new();

        public ClientTypeHandler(PacketMessageAttribute.PacketType ptype) : base(ptype)
        {
        }
        
        protected override Type GetReadType(ServerPackets id)
        {
            return typeof(PacketData);
        }

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
                    var handle = (IPacketMessage)Activator.CreateInstance(type);
                    _packetMessages.TryAdd((ServerPackets)(object)packetattributes[0].codeid,
                        async (data) => { await handle.Packet(data); });
                }
            }
        }


    }
    
    public class ClientPacket : FixeHeaderClientPacket
    {
        public IMessageTypeHeader TypeHeader { get; set; } = new ClientTypeHandler(PacketMessageAttribute.PacketType.ClientReciveType);

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
            // ushort 값 읽기
            ushort ushortValue = reader.ReadUInt16();

            // 남은 데이터를 ByteBuffer로 읽기
            
            // int remainingLength = (int)(reader.Length - reader.Position);
            // byte[] buffer = new byte[remainingLength];
            // reader.Read(buffer, 0, remainingLength);
            ByteBuffer byteBuffer = new ByteBuffer(reader.GetReadBuffers().Data);

            // CombinedData 객체 생성 및 반환
            PacketData data = new PacketData
            {
                ID = ushortValue,
                Data = byteBuffer
            };

            return data;
        }

        protected override void OnWrite(object data, IClient client, PipeStream writer)
        {
            // ToDo. ## 메모리 사용에 대한 최적화 필요성 있음 ##
            if (data is PacketData writedata)
            {
                // ushort 값 쓰기
                writer.Write(writedata.ID);

                // ByteBuffer 데이터 쓰기
                byte[] buffer = writedata.Data.ToFullArray();
                writer.Write(buffer, 0, buffer.Length);
            }
            else
            {
                throw new ArgumentException("데이터는 CombinedData 타입이어야 합니다.");
            }
        }
    }      
}
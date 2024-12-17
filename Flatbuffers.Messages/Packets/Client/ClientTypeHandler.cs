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
            // ushort 값 읽기
            ushort ushortValue = reader.ReadUInt16();
            int size = (int)reader.Length;
            PacketData data = new PacketData
            {
                ID = ushortValue,
                Data = new byte[size]
            };
            reader.Read(data.Data, 0, size);

            return data;
        }

        protected override void OnWrite(object data, IClient client, PipeStream writer)
        {
            // ToDo. ## 메모리 사용에 대한 최적화 필요성 있음 ##
            if (data is PacketData writedata)
            {
                // ushort 값 쓰기
                writer.Write((ushort)writedata.ID);

                // ByteBuffer 데이터 쓰기
                writer.Write(writedata.Data, 0, writedata.Data.Length);
            }
            else
            {
                throw new ArgumentException("데이터는 CombinedData 타입이어야 합니다.");
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
using System.Collections.Concurrent;
using System.Reflection;
using BeetleX;
using BeetleX.Buffers;
using BeetleX.EventArgs;
using BeetleX.Packets;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Flatbuffers.Messages.Packets.Server
{

    public class ServerTypeHandler : TypeHandler<ClientPackets, ServerPackets>
    {
        private ConcurrentDictionary<ClientPackets, Func<ISession,ByteBuffer, Task>> _packetMessages = new();

        public ServerTypeHandler(PacketMessageAttribute.PacketType ptype) : base(ptype)
        {
        }

        protected override Type GetReadType(ClientPackets id)
        {
            return typeof(PacketData);
        }
        
        public override void Register(params Assembly[] assemblies)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == false) continue;
                if (type.GetInterface("Network.Protocol.IPacketMessage.IServerPacketMessage") == null) continue;

                var packetattributes =
                    (ClientPacketMessageAttribute[])type.GetCustomAttributes(typeof(ClientPacketMessageAttribute),
                        true);
                if (packetattributes.Length > 0)
                {
                    var handle = (IServerPacketMessage)Activator.CreateInstance(type);
                    _packetMessages.TryAdd((ClientPackets)packetattributes[0].codeid,
                        async (session,data) => { await handle.Packet(session,data); });
                }
            }
        }        
    }
    
    
    public class ServerPacket : FixedHeaderPacket
    {
        public ServerPacket()
        {
            TypeHeader = new ServerTypeHandler(PacketMessageAttribute.PacketType.ServerReceiveType);
        }

        private PacketDecodeCompletedEventArgs mCompletedEventArgs = new PacketDecodeCompletedEventArgs();

        public void Register(params Assembly[] assemblies)
        {
            TypeHeader.Register(assemblies);
        }

        public IMessageTypeHeader TypeHeader { get; set; }

        public override IPacket Clone()
        {
            ServerPacket result = new ServerPacket();
            result.TypeHeader = TypeHeader;
            return result;
        }

        protected override object OnReader(ISession session, PipeStream reader)
        {
            // ushort 값 읽기
            ushort ushortValue = reader.ReadUInt16();
            int size = (int)reader.Length;
            // CombinedData 객체 생성 및 반환
            PacketData data = new PacketData
            {
                ID = ushortValue,
                Data = new byte[size]
            };
            
            reader.Read(data.Data, 0, size);
            return data;
        }

        protected override void OnWrite(ISession session, object data, PipeStream writer)
        {
            // ToDo. ## 메모리 사용에 대한 최적화 필요성 있음 ##
            if (data is PacketData writedata)
            {
                // ushort 값 쓰기
                writer.Write(writedata.ID);
                // ByteBuffer 데이터 쓰기
                writer.Write(writedata.Data, 0, writedata.Data.Length);
            }
            else
            {
                throw new ArgumentException("데이터는 CombinedData 타입이어야 합니다.");
            }
        }
    }    
}
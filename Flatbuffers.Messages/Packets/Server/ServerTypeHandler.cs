using System.Buffers;
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

        public ServerTypeHandler(PacketMessageAttribute.PacketType ptype) : base(ptype) { }
        protected override Type GetReadType(ClientPackets id) => typeof((ushort ID, ByteBuffer buffer));
        
        public override void Register(params Assembly[] assemblies)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == false) continue;
                if (type.GetInterface("IServerPacketMessage") == null) continue;

                var packetAttributes = type.GetCustomAttributes<ClientPacketMessageAttribute>(true);
                foreach (var attribute in packetAttributes)
                {
                    var handler = (IServerPacketMessage)Activator.CreateInstance(type);
                    _packetMessages.TryAdd((ClientPackets)attribute.codeid,
                        async (session, data) => await handler.Packet(session, data));
                }
                
                // var packetattributes =
                //     (ClientPacketMessageAttribute[])type.GetCustomAttributes(typeof(ClientPacketMessageAttribute),
                //         true);
                // if (packetattributes.Length > 0)
                // {
                //     var handle = (IServerPacketMessage)Activator.CreateInstance(type);
                //     _packetMessages.TryAdd((ClientPackets)packetattributes[0].codeid,
                //         async (session,data) => { await handle.Packet(session,data); });
                // }
            }
        }        
    }
    
    
    public class ServerPacket : FixedHeaderPacket
    {
        public ServerPacket()
        {
            TypeHeader = new ServerTypeHandler(PacketMessageAttribute.PacketType.ServerReceiveType);
        }

        private PacketDecodeCompletedEventArgs mCompletedEventArgs = new();
        public void Register(params Assembly[] assemblies) =>TypeHeader.Register(assemblies);
        public IMessageTypeHeader TypeHeader { get; set; }

        public override IPacket Clone()
        {
            return new ServerPacket { TypeHeader = TypeHeader };
        }

        protected override object OnReader(ISession session, PipeStream reader)
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

        protected override void OnWrite(ISession session, object data, PipeStream writer)
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
    }    
}
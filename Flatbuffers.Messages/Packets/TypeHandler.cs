using System.Collections.Concurrent;
using System.Reflection;
using BeetleX;
using BeetleX.Buffers;
using BeetleX.Clients;
using BeetleX.EventArgs;
using BeetleX.Packets;
using Google.FlatBuffers;
using Network.Protocol;
using NetworkMessage;

namespace Flatbuffers.Messages
{
    // 데이터 기본 형태
    public class PacketData
    {
        public ushort ID;
        public ByteBuffer Data;
    }
    
    // 타입 핸들러
    public abstract class TypeHandler<Recive,Send> : BeetleX.Packets.IMessageTypeHeader  
        where Recive : Enum
        where Send : Enum
    {
        
        private PacketMessageAttribute.PacketType packetType;
        
        public TypeHandler(PacketMessageAttribute.PacketType ptype)
        {
            packetType = ptype;
        }

        protected abstract Type GetReadType(Recive id);
        public Type ReadType(PipeStream reader)
        {
            return typeof(PacketData);
        }

        public void WriteType(object data, PipeStream stream)
        {

        }

        // 서버 / 클라에 맞는 recive 함수 등록
        public virtual void Register(params Assembly[] assemblies){}
    }
}

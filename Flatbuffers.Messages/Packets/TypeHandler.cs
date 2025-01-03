﻿using System.Reflection;
using BeetleX.Buffers;
using BeetleX.Packets;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;

namespace Flatbuffers.Messages
{
    // 타입 핸들러
    public abstract class TypeHandler<Receive,Send> : IMessageTypeHeader  
        where Receive : Enum
        where Send : Enum
    {
        
        private PacketMessageAttribute.PacketType packetType;
        
        public TypeHandler(PacketMessageAttribute.PacketType ptype)
        {
            packetType = ptype;
        }
        protected abstract Type GetReadType(Receive id);
        public Type ReadType(PipeStream reader)
        {
            return typeof((ushort ID, ByteBuffer buffer));
        }
        
        public void WriteType(object data, PipeStream stream)
        {

        }

        // 서버 / 클라에 맞는 recive 함수 등록
        public virtual void Register(params Assembly[] assemblies){}
    }
}

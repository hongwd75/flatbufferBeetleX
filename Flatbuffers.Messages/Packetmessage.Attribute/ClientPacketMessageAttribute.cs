using NetworkMessage;

namespace Flatbuffers.Messages
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class ClientPacketMessageAttribute  : PacketMessageAttribute
    {
        public override ushort codeid
        {
            get => (ushort)ID;
        }
        public readonly ClientPackets ID;
        public ClientPacketMessageAttribute(ClientPackets id) : base(PacketType.ServerReciveType)
        {
            ID = id;
        }
    }
}

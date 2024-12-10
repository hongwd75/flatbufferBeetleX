using NetworkMessage;

namespace Flatbuffers.Messages

{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class ServerPacketMessageAttribute : PacketMessageAttribute
    {
        public override ushort codeid
        {
            get => (ushort)ID;
        }        
        public readonly ServerPackets ID;
        public ServerPacketMessageAttribute(ServerPackets id) : base(PacketType.ClientReciveType)
        {
            ID = id;
        }
    }
}
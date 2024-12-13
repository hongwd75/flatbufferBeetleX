namespace Flatbuffers.Messages
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class PacketMessageAttribute : Attribute
    {
        public enum PacketType {
            ServerReceiveType = 0,
            ClientReceiveType,
        }

        public readonly PacketType receiveType;
        public virtual ushort codeid { get; }

        public PacketMessageAttribute(PacketType pt)
        {
            receiveType = pt;
        }
    }
}
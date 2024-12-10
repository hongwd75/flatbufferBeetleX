namespace Flatbuffers.Messages
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class PacketMessageAttribute : Attribute
    {
        public enum PacketType {
            ServerReciveType = 0,
            ClientReciveType,
        }

        public readonly PacketType reciveType;
        public virtual ushort codeid { get; }

        public PacketMessageAttribute(PacketType pt)
        {
            reciveType = pt;
        }
    }
}
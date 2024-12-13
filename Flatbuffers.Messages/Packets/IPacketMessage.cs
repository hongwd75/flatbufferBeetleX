using Google.FlatBuffers;
using System.Threading.Tasks;
using BeetleX;

namespace Network.Protocol.IPacketMessage
{
    public interface IClientPacketMessage
    {
        Task Packet(ByteBuffer buteBuffer);
    }
    
    public interface IServerPacketMessage
    {
        Task Packet(ISession session,ByteBuffer buteBuffer);
    }    
}
using Google.FlatBuffers;
using System.Threading.Tasks;
using BeetleX;

namespace Network.Protocol
{
    public interface IPacketMessage
    {
        Task Packet(ByteBuffer buteBuffer);
    }
    
    public interface IServerPacketMessage
    {
        Task Packet(ISession session,ByteBuffer buteBuffer);
    }    
}
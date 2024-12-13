// ** CS_WorldJoinReq 패킷 메시지
using System.Threading.Tasks;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ClientPacketMessageAttribute(ClientPackets.CS_WorldJoinReq)]
	public class CS_WorldJoinReq_handler : IServerPacketMessage
	{
		#pragma warning disable CS1998
		public async Task Packet(ISession session,ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			CS_WorldJoinReq_FBS packet = CS_WorldJoinReq.GetRootAsCS_WorldJoinReq(byteBuffer).UnPack();
			//Todo 코드 작업 필요
		}
	}
}

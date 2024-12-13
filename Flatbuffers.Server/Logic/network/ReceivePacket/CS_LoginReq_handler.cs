// ** CS_LoginReq 패킷 메시지
using System.Threading.Tasks;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ClientPacketMessageAttribute(ClientPackets.CS_LoginReq)]
	public class CS_LoginReq_handler : IServerPacketMessage
	{
		#pragma warning disable CS1998
		public async Task Packet(ISession session,ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			CS_LoginReq_FBS packet = CS_LoginReq.GetRootAsCS_LoginReq(byteBuffer).UnPack();
			//Todo 코드 작업 필요
		}
	}
}

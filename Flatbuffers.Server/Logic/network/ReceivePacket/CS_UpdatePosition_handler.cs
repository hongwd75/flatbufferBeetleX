// ** CS_UpdatePosition 패킷 메시지
using System.Threading.Tasks;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ClientPacketMessageAttribute(ClientPackets.CS_UpdatePosition)]
	public class CS_UpdatePosition_handler : IServerPacketMessage
	{
		#pragma warning disable CS1998
		public async Task Packet(ISession session,ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			CS_UpdatePosition_FBS packet = CS_UpdatePosition.GetRootAsCS_UpdatePosition(byteBuffer).UnPack();
			//Todo 코드 작업 필요
		}
	}
}

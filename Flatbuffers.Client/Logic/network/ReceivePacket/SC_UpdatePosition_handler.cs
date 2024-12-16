// ** SC_UpdatePosition 패킷 메시지
using System.Threading.Tasks;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ServerPacketMessageAttribute(ServerPackets.SC_UpdatePosition)]
	public class SC_UpdatePosition_handler : IClientPacketMessage
	{
		#pragma warning disable CS1998
		public async Task Packet(ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			SC_UpdatePosition_FBS packet = SC_UpdatePosition.GetRootAsSC_UpdatePosition(byteBuffer).UnPack();
			//Todo 코드 작업 필요
		}
	}
}

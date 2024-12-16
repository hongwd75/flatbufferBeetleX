// ** SC_RemovePlayer 패킷 메시지
using System.Threading.Tasks;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ServerPacketMessageAttribute(ServerPackets.SC_RemovePlayer)]
	public class SC_RemovePlayer_handler : IClientPacketMessage
	{
		#pragma warning disable CS1998
		public async Task Packet(ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			SC_RemovePlayer_FBS packet = SC_RemovePlayer.GetRootAsSC_RemovePlayer(byteBuffer).UnPack();
			//Todo 코드 작업 필요
		}
	}
}

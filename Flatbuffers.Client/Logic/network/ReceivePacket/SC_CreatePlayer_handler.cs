// ** SC_CreatePlayer 패킷 메시지
using System.Threading.Tasks;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ServerPacketMessageAttribute(ServerPackets.SC_CreatePlayer)]
	public class SC_CreatePlayer_handler : IClientPacketMessage
	{
		#pragma warning disable CS1998
		public async Task Packet(ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			SC_CreatePlayer_FBS packet = SC_CreatePlayer.GetRootAsSC_CreatePlayer(byteBuffer).UnPack();
			//Todo 코드 작업 필요
		}
	}
}

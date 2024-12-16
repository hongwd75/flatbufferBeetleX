// ** SC_CreatePlayers 패킷 메시지
using System.Threading.Tasks;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ServerPacketMessageAttribute(ServerPackets.SC_CreatePlayers)]
	public class SC_CreatePlayers_handler : IClientPacketMessage
	{
		#pragma warning disable CS1998
		public async Task Packet(ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			SC_CreatePlayers_FBS packet = SC_CreatePlayers.GetRootAsSC_CreatePlayers(byteBuffer).UnPack();
			//Todo 코드 작업 필요
		}
	}
}

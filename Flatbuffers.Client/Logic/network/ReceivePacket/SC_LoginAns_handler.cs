// ** SC_LoginAns 패킷 메시지
using System.Threading.Tasks;
using BeetleX;
using Google.FlatBuffers;
using Network.Protocol.IPacketMessage;
using NetworkMessage;

namespace Network.Protocol
{
	[ServerPacketMessageAttribute(ServerPackets.SC_LoginAns)]
	public class SC_LoginAns_handler : IClientPacketMessage
	{
		#pragma warning disable CS1998
		public async Task Packet(ByteBuffer byteBuffer)
		#pragma warning restore CS1998
		{
			SC_LoginAns_FBS packet = SC_LoginAns.GetRootAsSC_LoginAns(byteBuffer).UnPack();
			//Todo 코드 작업 필요
		}
	}
}

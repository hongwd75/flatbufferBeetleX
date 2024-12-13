using BeetleX;
using BeetleX.EventArgs;

namespace Game.Logic.network
{
    public class ServerNetworkHandler : ServerHandlerBase
    {
        //------------------------------------------------------------------------------------------------------
        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            // 패킷 처리 및 핸들러 연결            
        }

        public void Disconnect(GameClient client)
        {
        }
        
        //------------------------------------------------------------------------------------------------------
        #region OnConnect / OnDisconnect
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            base.Connected(server,e);
            Console.WriteLine("OnConnect : ");
        }
        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            base.Disconnect(server,e);

        }
        #endregion        
    }
}
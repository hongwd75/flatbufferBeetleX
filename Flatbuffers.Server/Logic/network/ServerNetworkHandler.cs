using BeetleX;
using BeetleX.EventArgs;

namespace Flatbuffers.Server.Logic.network
{
    public class ServerNetworkHandler : ServerHandlerBase
    {
        //------------------------------------------------------------------------------------------------------
        //
        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {

        }
        
        //------------------------------------------------------------------------------------------------------
        //
        public override void SessionReceive(IServer server, SessionReceiveEventArgs e)
        {
            base.SessionReceive(server, e);
        }

        //------------------------------------------------------------------------------------------------------
        #region OnConnect / OnDisconnect
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            base.Connected(server,e);
           
        }
        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            base.Disconnect(server,e);
        }
        #endregion        
    }
}
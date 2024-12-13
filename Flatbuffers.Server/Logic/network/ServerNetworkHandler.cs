using BeetleX;
using BeetleX.EventArgs;

namespace Game.Logic.network
{
    public class ServerNetworkHandler : ServerHandlerBase
    {
        public Action<IServer, PacketDecodeCompletedEventArgs> OnReceivePacket;
        public Action<IServer, ConnectedEventArgs> OnConnected;
        public Action<IServer, SessionEventArgs> OnDisconnected;
        
        //------------------------------------------------------------------------------------------------------
        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            OnReceivePacket?.Invoke(server,e);
        }

        public void Disconnect(GameClient client)
        {
            client.Session?.Dispose();
        }
        
        //------------------------------------------------------------------------------------------------------
        #region OnConnect / OnDisconnect
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            base.Connected(server,e);
            Console.WriteLine("OnConnect : ");
            OnConnected?.Invoke(server,e);
        }
        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            base.Disconnect(server,e);
            OnDisconnected?.Invoke(server,e);
        }
        #endregion        
    }
}
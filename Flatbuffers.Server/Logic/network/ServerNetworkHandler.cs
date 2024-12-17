using BeetleX;
using BeetleX.EventArgs;
using Flatbuffers.Messages;
using Google.FlatBuffers;
using NetworkMessage;

namespace Game.Logic.network
{
    public class ServerNetworkHandler : ServerHandlerBase
    {
        //------------------------------------------------------------------------------------------------------
        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            if (e.Message is (ushort ID, ByteBuffer buffer))
            {
                try
                {
                    GameServer.SendPacketClassMethods.OnReceivePacket(e.Session, (ClientPackets)ID, buffer);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }
        }

        public void Disconnect(GameClient client) => client.Session.Dispose();
        public void Disconnect(ISession client) => client.Dispose();
        
        //------------------------------------------------------------------------------------------------------
        #region OnConnect / OnDisconnect
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            base.Connected(server,e);
            e.Session.SocketProcessHandler = new OutPacket(e.Session);
        }
        
        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            base.Disconnect(server,e);
            e.Session.SocketProcessHandler = null;            
        }
        #endregion        
    }
}
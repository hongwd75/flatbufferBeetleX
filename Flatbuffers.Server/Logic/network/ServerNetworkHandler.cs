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
            if (e.Message is PacketData packetdata)
            {
                try
                {
                    GameServer.SendPacketClassMethods.OnReceivePacket(e.Session,
                        (ClientPackets)packetdata.ID,new ByteBuffer(packetdata.Data));
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }
        }

        public void Disconnect(GameClient client)
        {
            client.Session.Dispose();
        }
        public void Disconnect(ISession client)
        {
            client.Dispose();
        }        
        
        //------------------------------------------------------------------------------------------------------
        #region OnConnect / OnDisconnect
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            base.Connected(server,e);
            OutPacket processhandler = new OutPacket(e.Session);
            e.Session.SocketProcessHandler = processhandler;
            Console.WriteLine($"OnConnect : {e.Session.Host}");
        }
        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            base.Disconnect(server,e);
        }
        #endregion        
    }
}
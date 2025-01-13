using BeetleX;
using BeetleX.EventArgs;
using Flatbuffers.Messages;
using Google.FlatBuffers;
using NetworkMessage;

namespace Game.Logic.network
{
    public class ServerNetworkHandler : ServerHandlerBase
    {
        protected readonly HashSet<OutPacket> _clients = new HashSet<OutPacket>();
        protected readonly object _clientsLock = new object();

        public int ClientCount => GameServer.Instance.ServerSocket.Count;
        
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

        public void Disconnect(GameClient client)
        {
            Disconnect(client.Session);
        }

        public void Disconnect(ISession client)
        {
            if (client != null)
            {
                var outproc = (OutPacket)client.SocketProcessHandler;
                client.Dispose();
            }
        }
        
        //------------------------------------------------------------------------------------------------------
        #region OnConnect / OnDisconnect
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            base.Connected(server,e);
            OutPacketV1 output = new OutPacketV1(e.Session);
            e.Session.SocketProcessHandler = output;
            lock (_clientsLock)
            {
                _clients.Add(output);
            }
        }
        
        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            if (e.Session.SocketProcessHandler != null)
            {
                OutPacket output = (OutPacket)e.Session.SocketProcessHandler;
                lock (_clientsLock)
                {
                    output.OnDisconnect();
                    if (_clients.Contains(output))
                    {
                        _clients.Remove(output);
                    }
                }
            }

            base.Disconnect(server,e);
            e.Session.SocketProcessHandler = null;            
        }
        #endregion        
    }
}
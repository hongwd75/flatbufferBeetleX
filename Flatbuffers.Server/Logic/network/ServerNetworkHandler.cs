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

        public void Disconnect(GameClient client,bool removeClientArray)
        {
            Disconnect(client?.Session,removeClientArray);

        }

        public void Disconnect(ISession client,bool removeClientArray)
        {
            var outproc = (OutPacket)client.SocketProcessHandler;
            if (removeClientArray == true && outproc.Client != null)
            {
                
            }
            client.Dispose();   
        }
        
        //------------------------------------------------------------------------------------------------------
        #region OnConnect / OnDisconnect
        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            base.Connected(server,e);
            e.Session.SocketProcessHandler = new OutPacketV1(e.Session);
        }
        
        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            base.Disconnect(server,e);
            e.Session.SocketProcessHandler = null;            
        }
        #endregion        
    }
}
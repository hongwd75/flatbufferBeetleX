using BeetleX;
using BeetleX.EventArgs;
using System;
using System.Collections.Generic;
using Flatbuffers.Messages.Packets.Server;

namespace Flatbuffers.Server
{
    public class Program : ServerHandlerBase
    {
        private static IServer mServer;
        
        public static void Main(string[] args)
        {
            ServerOptions options = new ServerOptions();
            options.LogLevel = LogType.Info;
            mServer = SocketFactory.CreateTcpServer<Program, ServerPacket>(options);

            
            
            mServer.Open();    
            Console.Read();
        }
        
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
    }
}
using BeetleX;
using BeetleX.EventArgs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Flatbuffers.Messages.Packets.Server;
using Flatbuffers.Server.Logic.network;

namespace Flatbuffers.Server
{
    public class Program : ServerHandlerBase
    {
        private static IServer? mServerSocket;
        
        public static void Main(string[] args)
        {
            ServerOptions options = new ServerOptions();
            options.LogLevel = LogType.Info;
            mServerSocket = SocketFactory.CreateTcpServer<Program, ServerPacket>(options);

            if (mServerSocket.Handler is Program program)
            {
                program.Start();
            }
            mServerSocket.Log(LogType.Warring,null,"----- 서버 시작 ------");
            Console.Read();
        }

        //------------------------------------------------------------------------------------------------------
        //        
        public void Start()
        {
            GameClient.SendPacketClassMethods.Register();
            mServerSocket?.Open();
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
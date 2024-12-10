using BeetleX;
using BeetleX.Clients;
using System;
using System.Collections.Generic;
using BeetleX.EventArgs;
using Flatbuffers.Messages.Packets.Client;

namespace Game.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientPacket packet = new ClientPacket();
            packet.Register(null);
            BeetleX.Clients.TcpClient client = BeetleX.SocketFactory.CreateClient<TcpClient>(packet, "127.0.0.1", 9090);
            client.Connect();
            //TcpClient client = SocketFactory.CreateClient<TcpClient>(packet, "127.0.0.1", 9090,"localhost");
            while (true)
            {
                Console.Read();
            }
        }
    }    
}
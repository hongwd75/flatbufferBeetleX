using BeetleX;
using BeetleX.Clients;
using System;
using System.Collections.Generic;
using BeetleX.EventArgs;
using Flatbuffers.Messages.Packets.Client;
using Game.Logic.ConsoleMake;
using Game.Logic;
using Google.FlatBuffers;
using NetworkMessage;

namespace Game.Logic.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // ConsoleMakeServerPacketHandle.make();
            // return;
            ClientNetworkLogic logic = new ClientNetworkLogic();
            logic.Init("127.0.0.1", 10300);
            while (true)
            {
                Console.Write("메시지 :");
                string line = Console.ReadLine();
                int category;
                if (int.TryParse(line, out category))
                {
                    if (category == 1)
                    {
                        logic.SendLoginReq("abcd","1111");
                    }
                }
            }
        }
    }    
}
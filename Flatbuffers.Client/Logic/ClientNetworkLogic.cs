using System.Runtime.Serialization.Formatters.Binary;
using BeetleX;
using BeetleX.Clients;
using Flatbuffers.Messages;
using Flatbuffers.Messages.Packets.Client;
using Game.Client.Network;
using Google.FlatBuffers;
using NetworkMessage;

namespace Game.Client.Logic;

public class ClientNetworkLogic
{
    private ClientPacketMethodsManager sendPacketMethods;
    private ClientPacket Packet;
    private AsyncTcpClient client;

    public void Init(string url, int port)
    {
        Packet = new ClientPacket();
        Packet.Register(null);
        
        sendPacketMethods = new ClientPacketMethodsManager();
        sendPacketMethods.Register();

        client = SocketFactory.CreateClient<AsyncTcpClient>(Packet, url, port);
        client.PacketReceive = OnPacket;
        client.Connect();
    }
    
    public void Send(ClientPackets sc, byte[] buffer)
    {
        client.Send(((ushort)sc,buffer));
    }
    
    public void SendLoginReq(string id, string pwd)
    {
        CS_LoginReq_FBS req = new CS_LoginReq_FBS();
        req.Id = id;
        req.Pwd = pwd;
        FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
        var packfunc = sendPacketMethods.GetClientPacketType(ClientPackets.CS_LoginReq, req);
        object packedOffset = packfunc.method.Invoke(packfunc.obj, new object[] { sendBuilder, req });
        sendBuilder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
        Send(ClientPackets.CS_LoginReq,sendBuilder.SizedByteArray());
    }
    
    private void OnPacket(IClient client, object message)
    {
        if (message is (ushort ID, ByteBuffer buffer))
        {
            ClientTypeHandler handler = (ClientTypeHandler)Packet.TypeHeader;
            if (handler.PacketMsg.TryGetValue((ServerPackets)ID, out var fnc) == true)
            {
                fnc.Invoke(buffer);
            }
        }
    }
}
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
    private TcpClient client;

    public void Init(string url, int port)
    {
        Packet = new ClientPacket();
        Packet.Register(null);
        Packet.Completed += OnPacket;
        
        sendPacketMethods = new ClientPacketMethodsManager();
        sendPacketMethods.Register();

        client = SocketFactory.CreateClient<TcpClient>(Packet, url, port);
        client.Connect();
    }

    
    public void Send(ClientPackets sc, ByteBuffer buffer)
    {
        PacketData obj = new PacketData()
        {
            ID = (ushort)sc,
            Data = buffer
        };
        client.SendMessage(obj);
    }
    
    public void SendLoginReq(string id, string pwd)
    {
        CS_LoginReq_FBS req = new CS_LoginReq_FBS();
        req.Id = id;
        req.Pwd = pwd;
        FlatBufferBuilder sendBuilder = new FlatBufferBuilder(1024);
        var packedOffset = sendPacketMethods.GetClientPacketType(ClientPackets.CS_LoginReq, req);
        sendBuilder.Finish((int)packedOffset.GetType().GetField("Value").GetValue(packedOffset));
        
        Send(ClientPackets.CS_LoginReq,sendBuilder.DataBuffer);
    }
    
    private void OnPacket(IClient client, object message)
    {
        PacketData data = (PacketData)message;
        Packet.OnRecieveData((ServerPackets)data.ID,data.Data);
    }
}
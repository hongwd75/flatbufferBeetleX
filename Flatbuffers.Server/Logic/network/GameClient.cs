using BeetleX;
using Flatbuffers.Messages.Packets.Client;

namespace Flatbuffers.Server.Logic.network;

public class GameClient
{
    protected IServer? m_server;
    protected ISession? m_session;

    public static ClientPacketMethodsManager SendPacketClassMethods = new ClientPacketMethodsManager();

    public void SetSession(ISession session)
    {
        m_session = session;
    }

    public void Send<T>(ClientPacket id, T message)
    {
        
    }
}
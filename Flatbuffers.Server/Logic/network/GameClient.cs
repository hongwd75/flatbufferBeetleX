using BeetleX;
using Flatbuffers.Messages.Packets.Client;
using Game.Logic;
using Logic.database.table;

namespace Flatbuffers.Server.Logic.network;

public class GameClient
{
    #region eClientState enum
    public enum eClientState
    {
        NotConnected = 0x00,
        Connecting = 0x01,
        CharScreen = 0x02,
        WorldEnter = 0x03,
        Playing = 0x04,
        Linkdead = 0x05,
        Disconnected = 0x06,
    } ;
    #endregion
    
    protected ISession mSession;
    protected GamePlayer mPlayer;
    protected Account mAccount;
    protected int mGamePlayerArrayID;
    public static ClientPacketMethodsManager SendPacketClassMethods = new ClientPacketMethodsManager();

    #region GET / SET
    public GamePlayer Player
    {
        get => mPlayer;
        set
        {
            mPlayer = value;
        }
    }
    public ISession Session
    {
        get => mSession;
        set
        {
            mSession = value;
        }
    }
    public int PlayerArrayID
    {
        get => mGamePlayerArrayID;
        set
        {
            mGamePlayerArrayID = value;
        }
    }

    public Account Account
    {
        get => mAccount;
        set
        {
            mAccount = value;
        }
    }
    #endregion

    public GameClient(ISession session)
    {
        mSession = session;
    }


    public void Send<T>(ClientPacket id, T message)
    {
        
    }
}
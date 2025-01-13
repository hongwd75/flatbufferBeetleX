using BeetleX;
using Game.Logic.network;
using Logic.database.table;

namespace Game.Logic.managers;

public class GameClientManager : IDisposable
{
    public const long PING_TIMEOUT = 360;
    public const long PING_TIME = 10 * 60 * 10000000L;
    private GameClient[] mClients = Array.Empty<GameClient>();
    private Timer m_pingCheckTimer;
    public GameClientManager(int maxClient)
    {
        mClients = new GameClient[maxClient];
        m_pingCheckTimer = new Timer(new TimerCallback(PingCheck), null, 10 * 1000, 0); // every 10s a check
    }

    public void Dispose()
    {
        if (m_pingCheckTimer != null)
        {
            m_pingCheckTimer.Dispose();
            m_pingCheckTimer = null;
        }        
    }
    
    public IList<GameClient> GetAllClients()
    {
        lock (mClients.SyncRoot)
        {
            return mClients.Where(c => c != null).ToList();
        }
    }
    public IList<GameClient> GetAllPlayingClients()
    {
        var targetClients = new List<GameClient>();

        lock (mClients.SyncRoot)
        {
            foreach (GameClient client in mClients)
            {
                if (client != null
                    && client.IsPlaying
                    && client.Player != null
                    && client.Player.ObjectState == GameObject.eObjectState.Active)
                    targetClients.Add(client);
            }
        }
        return targetClients;
    }    
    
    public GameClient GetClientByAccountName(string accountName, bool exactMatch)
    {
        accountName = accountName.ToLower();
        lock (mClients.SyncRoot)
        {
            foreach (GameClient client in mClients)
            {
                if (client != null)
                {
                    if ((exactMatch && client.Account.Name.ToLower() == accountName)
                        || (!exactMatch && client.Account.Name.ToLower().StartsWith(accountName)))
                    {
                        return client;
                    }
                }
            }
        }
        return null;
    }

    public GameClient CreateAccount(Account account,ISession session)
    {
        lock (mClients.SyncRoot)
        {
            for (int i = 0; i < mClients.Length; i++)
            {
                if (mClients[i] == null)
                {
                    GameClient obj = new GameClient(session);
                    obj.Account = account;
                    obj.PlayerArrayID = i;
                    mClients[i] = obj;
                    return obj;
                }
            }
        }
        return null;
    }

    public void Remove(int arrayindex)
    {
        lock (mClients.SyncRoot)
        {
            if (mClients[arrayindex] != null)
            {
                mClients[arrayindex] = null;
            }
        }
    }
    
    private void PingCheck(object sender)
    {
        try
        {
            foreach (GameClient client in GetAllClients())
            {
                try
                {
                    // check ping timeout if we are in charscreen or in playing state
                    if (client.ClientState == GameClient.eClientState.Playing)
                    {
                        if (client.PingTime + PING_TIMEOUT * 1000 * 1000 * 10 < DateTime.Now.Ticks) // 1시간
                        {
                            GameServer.Instance.NetworkHandler.Disconnect(client);
                        }
                    }
                    else
                    {
                        // in all other cases client gets 10min to get wether in charscreen or playing state
                        if (client.PingTime + PING_TIME < DateTime.Now.Ticks)
                        {
                            GameServer.Instance.NetworkHandler.Disconnect(client);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // if (log.IsErrorEnabled)
                    //     log.Error("PingCheck", ex);
                }
            }
        }
        catch (Exception e)
        {
            // if (log.IsErrorEnabled)
            //     log.Error("PingCheck callback", e);
        }
        finally
        {
            m_pingCheckTimer.Change(10 * 1000, Timeout.Infinite);
        }
    }    
}
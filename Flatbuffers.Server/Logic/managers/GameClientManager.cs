using Game.Logic.network;

namespace Game.Logic.managers;

public class GameClientManager
{
    public const long PING_TIMEOUT = 360; 
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
    private void PingCheck(object sender)
    {
        try
        {
            foreach (GameClient client in GetAllClients())
            {
                try
                {
                    // check ping timeout if we are in charscreen or in playing state
                    if (client.ClientState == GameClient.eClientState.CharScreen ||
                        client.ClientState == GameClient.eClientState.Playing)
                    {
                        if (client.PingTime + PING_TIMEOUT * 1000 * 1000 * 10 < DateTime.Now.Ticks)
                        {
                            GameServer.Instance.NetworkHandler.Disconnect(client);
                        }
                    }
                    else
                    {
                        // in all other cases client gets 10min to get wether in charscreen or playing state
                        if (client.PingTime + 10 * 60 * 10000000L < DateTime.Now.Ticks)
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
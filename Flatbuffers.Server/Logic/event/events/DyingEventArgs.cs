namespace Game.Logic.Events;

public class DyingEventArgs : EventArgs
{
    private GameObject m_killer;
    private List<GamePlayer> m_playerKillers = null;
    
    public DyingEventArgs(GameObject killer)
    {
        m_killer=killer;
    }

    public DyingEventArgs(GameObject killer, List<GamePlayer> playerKillers)
    {
        m_killer = killer;
        m_playerKillers = playerKillers;
    }

    public GameObject Killer
    {
        get { return m_killer; }
    }

    public List<GamePlayer> PlayerKillers
    {
        get { return m_playerKillers; }
    }
}
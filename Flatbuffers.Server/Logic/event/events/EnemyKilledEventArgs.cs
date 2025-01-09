namespace Game.Logic.Events;

public class EnemyKilledEventArgs : EventArgs
{
    private readonly GameLiving m_target;
    public EnemyKilledEventArgs(GameLiving target)
    {
        this.m_target=target;
    }

    public GameLiving Target
    {
        get { return m_target; }
    }
}
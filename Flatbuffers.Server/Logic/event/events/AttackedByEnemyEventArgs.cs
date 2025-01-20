using Game.Logic.Skills;

namespace Game.Logic.Events;

public class AttackedByEnemyEventArgs : EventArgs
{

    /// <summary>
    /// The attack data
    /// </summary>
    private AttackData m_attackData;

    /// <summary>
    /// Constructs a new AttackedByEnemy
    /// </summary>
    public AttackedByEnemyEventArgs(AttackData attackData)
    {
        this.m_attackData=attackData;
    }

    /// <summary>
    /// Gets the attack data
    /// </summary>
    public AttackData AttackData
    {
        get { return m_attackData; }
    }
}
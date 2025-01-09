using Game.Logic.PropertyCalc;

namespace Game.Logic.Events;

public class EnemyHealedEventArgs : EventArgs
{
    private readonly GameLiving m_enemy;
    private readonly GameObject m_healSource;
    private readonly eChargeChangeType m_changeType;
    private readonly int m_healAmount;
    
    public EnemyHealedEventArgs(GameLiving enemy, GameObject healSource, eChargeChangeType changeType, int healAmount)
    {
        m_enemy = enemy;
        m_healSource = healSource;
        m_changeType = changeType;
        m_healAmount = healAmount;
    }
    
    public GameLiving Enemy
    {
        get { return m_enemy; }
    }

    public GameObject HealSource
    {
        get { return m_healSource; }
    }

    public eChargeChangeType ChangeType
    {
        get { return m_changeType; }
    }

    public int HealAmount
    {
        get { return m_healAmount; }
    }
}
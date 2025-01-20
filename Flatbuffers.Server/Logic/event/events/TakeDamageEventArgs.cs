namespace Game.Logic.Events;

public class TakeDamageEventArgs : EventArgs
{
    private GameObject m_damageSource;
    private eDamageType m_damageType;
    private int m_damageAmount;
    private int m_criticalAmount;

    public TakeDamageEventArgs(GameObject damageSource, eDamageType damageType, int damageAmount, int criticalAmount)
    {
        m_damageSource = damageSource;
        m_damageType = damageType;
        m_damageAmount = damageAmount;
        m_criticalAmount = criticalAmount;
    }

    public GameObject DamageSource => m_damageSource;
    public eDamageType DamageType => m_damageType;
    public int DamageAmount => m_damageAmount;
    public int CriticalAmount => m_criticalAmount;
}
using Game.Logic.PropertyCalc;

namespace Game.Logic.Events;

public class HealthChangedEventArgs : EventArgs
{
    private GameObject m_changesource;
    private eChargeChangeType m_changetype;
    private int m_changeamount;

    public HealthChangedEventArgs(GameObject source, eChargeChangeType type, int amount)
    {
        m_changesource = source;
        m_changetype = type;
        m_changeamount = amount;
    }

    public GameObject ChangeSource
    {
        get { return m_changesource; }
    }

    public eChargeChangeType ChangeType
    {
        get { return m_changetype; }
    }

    public int ChangeAmount
    {
        get { return m_changeamount; }
    }
}
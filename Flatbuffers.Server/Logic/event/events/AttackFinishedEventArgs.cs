using Game.Logic.Skills;

namespace Game.Logic.Events;

public class AttackFinishedEventArgs : EventArgs
{
    private AttackData m_attackData;
    public AttackFinishedEventArgs(AttackData attackData)
    {
        this.m_attackData=attackData;
    }
    public AttackData AttackData
    {
        get { return m_attackData; }
    }
}
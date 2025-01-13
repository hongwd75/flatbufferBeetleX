using Game.Logic.Spells;

namespace Game.Logic.Events;

public class CastFailedEventArgs : CastingEventArgs
{
    public enum Reasons
    {
        TargetTooFarAway,
        TargetNotInView,
        AlreadyCasting,
        CrowdControlled,
        NotEnoughPower,
    };

    public CastFailedEventArgs(ISpellHandler handler, Reasons reason) 
        : base(handler)
    {
        this.m_reason = reason;
    }

    private Reasons m_reason;

    public Reasons Reason
    {
        get { return m_reason; }
    }
}
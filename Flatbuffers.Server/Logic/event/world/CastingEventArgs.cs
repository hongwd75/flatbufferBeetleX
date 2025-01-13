using Game.Logic.Skills;
using Game.Logic.Spells;

namespace Game.Logic.Events;

public class CastingEventArgs : EventArgs
{
    private ISpellHandler m_handler;
    private GameLiving m_target = null;
    private AttackData m_lastAttackData = null;

    public CastingEventArgs(ISpellHandler handler)
    {
        this.m_handler = handler;
    }

    public CastingEventArgs(ISpellHandler handler, GameLiving target)
    {
        this.m_handler = handler;
        this.m_target = target;
    }

    public CastingEventArgs(ISpellHandler handler, GameLiving target, AttackData ad)
    {
        this.m_handler = handler;
        this.m_target = target;
        m_lastAttackData = ad;
    }

    public ISpellHandler SpellHandler
    {
        get { return m_handler; }
    }

    public GameLiving Target
    {
        get { return m_target; }
    }

    public AttackData LastAttackData
    {
        get { return m_lastAttackData; }
    }
}
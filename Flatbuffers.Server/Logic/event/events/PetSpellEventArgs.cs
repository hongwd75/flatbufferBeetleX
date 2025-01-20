using Game.Logic.Skills;

namespace Game.Logic.Events;

class PetSpellEventArgs : EventArgs
{
    private Spell m_spell;
    private SpellLine m_spellLine;
    private GameLiving m_target;

    public PetSpellEventArgs(Spell spell, SpellLine spellLine, GameLiving target)
    {
        m_spell = spell;
        m_spellLine = spellLine;
        m_target = target;
    }

    public Spell Spell
    {
        get { return m_spell; }
    }

    public SpellLine SpellLine
    {
        get { return m_spellLine; }
    }

    public GameLiving Target
    {
        get { return m_target; }
    }
}
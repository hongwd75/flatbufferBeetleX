namespace Game.Logic.Skills;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SpellHandlerAttribute : Attribute
{
    string m_type;

    public SpellHandlerAttribute(string spellType) {
        m_type = spellType;
    }

    public string SpellType {
        get { return m_type; }
    }
}
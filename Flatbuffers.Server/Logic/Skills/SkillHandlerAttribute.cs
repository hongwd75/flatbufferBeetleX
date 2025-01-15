namespace Game.Logic.Skills;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SkillHandlerAttribute : Attribute
{
    protected string m_keyName;

    public SkillHandlerAttribute(string keyName)
    {
        m_keyName = keyName;
    }

    public string KeyName
    {
        get { return m_keyName; }
    }
}
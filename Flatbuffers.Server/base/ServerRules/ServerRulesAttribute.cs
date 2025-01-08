namespace Game.Logic.ServerRules;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ServerRulesAttribute : Attribute
{
    protected eGameServerType m_serverType;

    public eGameServerType ServerType
    {
        get { return m_serverType; }
    }

    public ServerRulesAttribute(eGameServerType serverType)
    {
        m_serverType = serverType;
    }
}
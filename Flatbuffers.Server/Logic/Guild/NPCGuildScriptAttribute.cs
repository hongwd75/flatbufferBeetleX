using Game.Logic.World;

namespace Game.Logic.Guild;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NPCGuildScriptAttribute : Attribute
{
    string m_guild;
    eRealm m_realm;

    public NPCGuildScriptAttribute(string guildname, eRealm realm)
    {
        m_guild = guildname;
        m_realm = realm;
    }

    public NPCGuildScriptAttribute(string guildname)
    {
        m_guild = guildname;
        m_realm = eRealm.None;
    }

    public string GuildName {
        get { return m_guild; }
    }

    public eRealm Realm {
        get { return m_realm; }
    }
}
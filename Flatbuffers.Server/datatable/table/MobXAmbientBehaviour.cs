using Logic.database.attribute;

namespace Logic.database.table;

[DataTable(TableName = "MobXAmbientBehaviour", PreCache = true)]
public class MobXAmbientBehaviour : DataObject
{
    private string m_source;
    private string m_trigger;
    private ushort m_emote;
    private string m_text;
    private ushort m_chance;
    private string m_voice;
    
    public MobXAmbientBehaviour()
    {
        m_source = string.Empty;
        m_trigger =string.Empty;
        m_emote = 0;
        m_text = string.Empty;
        m_chance = 0;
        m_voice = string.Empty;
    }

    public MobXAmbientBehaviour(string name, string trigger, ushort emote, string text, ushort chance, string voice)
    {
        m_source = name;
        m_trigger = trigger;
        m_emote = emote;
        m_text = text;
        m_chance = chance;
        m_voice = voice;
    }

    [DataElement(AllowDbNull = false, Index = true)]
    public string Source
    {
        get { return m_source; }
        set { m_source = value; }
    }

    [DataElement(AllowDbNull = false)]
    public string Trigger
    {
        get { return m_trigger; }
        set { m_trigger = value; }
    }

    [DataElement(AllowDbNull = false)]
    public ushort Emote
    {
        get { return m_emote; }
        set { m_emote = value; }
    }

    [DataElement(AllowDbNull = false)]
    public string Text
    {
        get { return m_text; }
        set { m_text = value; }
    }
	
    [DataElement(AllowDbNull = false)]
    public ushort Chance
    {
        get { return m_chance; }
        set { m_chance = value; }
    }

    [DataElement(AllowDbNull = true)]
    public string Voice
    {
        get { return m_voice; }
        set { m_voice = value; }
    }
}
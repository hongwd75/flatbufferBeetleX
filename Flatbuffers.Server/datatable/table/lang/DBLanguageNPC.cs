﻿using Game.Logic.datatable;
using Logic.database.attribute;

namespace Logic.database.table;

[DataTable(TableName = "LanguageNPC")]
public class DBLanguageNPC : LanguageDataObject
{
    #region Variables
    private string m_name = string.Empty;
    private string m_suffix = string.Empty;
    private string m_guildName = string.Empty;
    private string m_examineArticle = string.Empty;
    private string m_messageArticle = string.Empty;
    #endregion Variables

    public DBLanguageNPC()
        : base() { }

    #region Properties
    public override eTranslationIdentifier TranslationIdentifier
    {
        get { return eTranslationIdentifier.eNPC; }
    }

    /// <summary>
    /// Gets or sets the translated name.
    /// </summary>
    [DataElement(AllowDbNull = true)]
    public string Name
    {
        get { return m_name; }
        set
        {
            Dirty = true;
            m_name = value;
        }
    }

    /// <summary>
    /// Gets or sets the name suffix (currently used by necromancer pets).
    /// 
    /// The XYZ spell is no longer in the Death Servant's queue.
    /// 
    /// 's = the suffix.
    /// </summary>
    [DataElement(AllowDbNull = true)]
    public string Suffix
    {
        get { return m_suffix; }
        set
        {
            Dirty = true;
            m_suffix = value;
        }
    }

    /// <summary>
    /// Gets or sets the translated guild name.
    /// </summary>
    [DataElement(AllowDbNull = true)]
    public string GuildName
    {
        get { return m_guildName; }
        set
        {
            Dirty = true;
            m_guildName = value;
        }
    }

    /// <summary>
    /// Gets or sets the translated examine article.
    /// 
    /// You examine the Tree.
    /// 
    /// the = the examine article.
    /// </summary>
    [DataElement(AllowDbNull = true)]
    public string ExamineArticle
    {
        get { return m_examineArticle; }
        set
        {
            Dirty = true;
            m_examineArticle = value;
        }
    }

    /// <summary>
    /// Gets or sets the translated message article.
    /// 
    /// GamePlayer has been killed by a Tree.
    /// 
    /// a = the message article.
    /// </summary>
    [DataElement(AllowDbNull = true)]
    public string MessageArticle
    {
        get { return m_messageArticle; }
        set
        {
            Dirty = true;
            m_messageArticle = value;
        }
    }
    #endregion Properties
}
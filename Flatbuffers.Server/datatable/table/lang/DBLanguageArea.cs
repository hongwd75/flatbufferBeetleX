﻿using Game.Logic.datatable;
using Logic.database.attribute;

namespace Logic.database.table;

[DataTable(TableName = "LanguageArea")]
public class DBLanguageArea : LanguageDataObject
{
    #region Variables
    private string m_description;
    private string m_screenDescription;
    #endregion Variables

    public DBLanguageArea()
        : base() { }

    #region Properties
    public override eTranslationIdentifier TranslationIdentifier
    {
        get { return eTranslationIdentifier.eArea; }
    }

    /// <summary>
    /// The translated area description
    /// </summary>
    [DataElement(AllowDbNull = false)]
    public string Description
    {
        get { return m_description; }
        set
        {
            Dirty = true;
            m_description = value;
        }
    }

    /// <summary>
    /// The translated area screen description
    /// </summary>
    [DataElement(AllowDbNull = false)]
    public string ScreenDescription
    {
        get { return m_screenDescription; }
        set
        {
            Dirty = true;
            m_screenDescription = value;
        }
    }
    #endregion Properties
}
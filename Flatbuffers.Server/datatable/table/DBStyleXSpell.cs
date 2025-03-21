﻿using Logic.database.attribute;

namespace Logic.database.table;

[DataTable(TableName = "StyleXSpell")]
public class DBStyleXSpell : DataObject
{
    protected int m_SpellID;
    protected int m_ClassID;
    protected int m_StyleID;
    protected int m_Chance;


    /// <summary>
    /// The Constructor
    /// </summary>
    public DBStyleXSpell()
        : base()
    {
        AllowAdd = false;
    }

    /// <summary>
    /// The Spell ID
    /// </summary>
    [DataElement(AllowDbNull = false)]
    public int SpellID
    {
        get { return m_SpellID; }
        set { m_SpellID = value; Dirty = true; }
    }

    /// <summary>
    /// The ClassID, required for style subsitute procs (0 is not a substitute style)
    /// </summary>
    [DataElement(AllowDbNull = false)]
    public int ClassID
    {
        get { return m_ClassID; }
        set { m_ClassID = value; Dirty = true; }
    }

    /// <summary>
    /// The StyleID
    /// </summary>
    [DataElement(AllowDbNull = false, Index = true)]
    public int StyleID
    {
        get { return m_StyleID; }
        set { m_StyleID = value; Dirty = true; }
    }

    /// <summary>
    /// The Chance to add to the styleeffect list
    /// </summary>
    [DataElement(AllowDbNull = false)]
    public int Chance
    {
        get { return m_Chance; }
        set { m_Chance = value; Dirty = true; }
    }
}
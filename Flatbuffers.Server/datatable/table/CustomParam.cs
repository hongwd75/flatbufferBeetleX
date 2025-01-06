using Logic.database.attribute;

namespace Logic.database.table;

public class CustomParam : DataObject
{
    private string m_keyName;
		
    /// <summary>
    /// KeyName for referencing this value.
    /// </summary>
    [DataElement(AllowDbNull = false, Varchar = 100, Index = true)]
    public string KeyName {
        get => m_keyName;
        set => SetProperty(ref m_keyName, value);           
    }
		
    private string m_value;
		
    /// <summary>
    /// Value, can be converted to numeric from string value.
    /// </summary>
    [DataElement(AllowDbNull = true, Varchar = 255)]
    public string Value {
        get => m_value;
        set => SetProperty(ref m_value, value);        
    }
		
    private int m_CustomParamID;
		
    /// <summary>
    /// Primary Key Auto Inc
    /// </summary>
    [PrimaryKey(AutoIncrement = true)]
    public int CustomParamID {
        get => m_CustomParamID;
        set => SetProperty(ref m_CustomParamID, value);        
    }

    /// <summary>
    /// Create new instance of <see cref="CustomParam"/> implementation
    /// Need to be linked with Foreign Key in subclass
    /// </summary>
    /// <param name="KeyName">Key Name</param>
    /// <param name="Value">Value</param>
    protected CustomParam(string KeyName, string Value)
    {
        this.KeyName = KeyName;
        this.Value = Value;
    }
		
    /// <summary>
    /// Default constructor for <see cref="CustomParam"/>
    /// </summary>
    protected CustomParam()
    {
    }    
}
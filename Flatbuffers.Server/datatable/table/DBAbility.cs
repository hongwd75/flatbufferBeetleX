using Logic.database.attribute;

namespace Logic.database.table;

[DataTable(TableName="Ability")]
public class DBAbility : DataObject
{
	protected int	 m_abilityID;
	
	protected string m_keyName;
	protected int	m_iconID = 0;		// 0 if no icon, ability icons start at 0x190
	protected int m_internalID;
	protected string m_name = "unknown";
	protected string m_description = "no description";
	protected string m_implementation = null;

	/// <summary>
	/// Create ability
	/// </summary>
	public DBAbility()
	{
	}

	/// <summary>
	/// Ability Primary Key Auto Increment
	/// </summary>
	[PrimaryKey(AutoIncrement=true)]
	public int AbilityID {
		get { return m_abilityID; }
		set { m_abilityID = value; }
	}

	
	/// <summary>
	/// The key of this ability
	/// </summary>
	[DataElement(AllowDbNull=false, Unique=true, Varchar=100)]
	public string KeyName
	{
		get {	return m_keyName;	}
		set	{
			Dirty = true;
			m_keyName = value;
		}
	}

	/// <summary>
	/// Name of this ability
	/// </summary>
	[DataElement(AllowDbNull=false, Varchar=255)]
	public string Name
	{
		get {	return m_name;	}
		set	{
			Dirty = true;
			m_name = value;
		}
	}

	/// <summary>
	/// Ability ID (new in 1.112)
	/// </summary>
	[DataElement(AllowDbNull=false)]
	public int InternalID
	{
		get { return m_internalID; }
		set
		{
			Dirty = true;
			m_internalID = value;
		}
	}
	
	/// <summary>
	/// Small description of this ability
	/// </summary>
	[DataElement(AllowDbNull=false)]
	public string Description
	{
		get {	return m_description;	}
		set
		{
			Dirty = true;
			m_description = value;
		}
	}

	/// <summary>
	/// Icon of ability
	/// </summary>
	[DataElement(AllowDbNull=false)]
	public int IconID
	{
		get {	return m_iconID;	}
		set
		{
			Dirty = true;
			m_iconID = value;
		}
	}

	/// <summary>
	/// Ability Implementation Class
	/// </summary>
	[DataElement(AllowDbNull=true, Varchar=255)]
	public string Implementation
	{
		get {	return m_implementation;	}
		set
		{
			Dirty = true;
			m_implementation = value;
		}
	}
}
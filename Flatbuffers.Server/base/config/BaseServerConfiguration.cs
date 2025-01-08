using System.Net;
using System.Reflection;
using Game.Logic;
using Logic.database.connection;

namespace Server.Config
{
public class BaseServerConfiguration
	{
		/// <summary>
		/// The listening address of the server.
		/// </summary>
		private IPAddress _ip;

		/// <summary>
		/// The listening port of the server.
		/// </summary>
		private ushort _port;
		
		/// <summary>
		/// The count of server cpu
		/// </summary>
		protected int m_cpuCount;
		protected int m_cpuUse;
		
		/// <summary>
		/// The max client count.
		/// </summary>
		protected int m_maxClientCount;
		
		/// <summary>
		/// holds the server root directory
		/// </summary>
		protected string m_rootDirectory;

		/// <summary>
		/// Holds the log configuration file path
		/// </summary>
		protected string m_logConfigFile;
		
		/// <summary>
		/// True if the server shall automatically create accounts
		/// </summary>
		protected bool m_autoAccountCreation;
		
		#region Database

		/// <summary>
		/// The path to the XML database folder
		/// </summary>
		protected string m_dbConnectionString;

		/// <summary>
		/// Type database type
		/// </summary>
		protected ConnectionType m_dbType;

		/// <summary>
		/// True if the server shall autosave the db
		/// </summary>
		protected bool m_autoSave;

		/// <summary>
		/// The auto save interval in minutes
		/// </summary>
		protected int m_saveInterval;

		#endregion		
		/// <summary>
		/// Constructs a server configuration with default values.
		/// </summary>
		public BaseServerConfiguration()
		{
			_port = 10300;
			_ip = IPAddress.Any;
			
			if (Assembly.GetEntryAssembly() != null)
				m_rootDirectory = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
			else
				m_rootDirectory = new FileInfo(Assembly.GetAssembly(typeof(GameServer)).Location).DirectoryName;
			
			m_logConfigFile = Path.Combine(Path.Combine(".", "config"), "logconfig.xml");
			
			m_autoAccountCreation = true;
			m_dbType = ConnectionType.DATABASE_SQLITE;
			m_dbConnectionString = $"Data Source={Path.Combine(m_rootDirectory, "dol.sqlite3.db")}";
			m_autoSave = true;
			m_saveInterval = 10;
			m_maxClientCount = 500;

			// Get count of CPUs
			m_cpuCount = Environment.ProcessorCount;
			if (m_cpuCount < 1)
			{
				try
				{
					m_cpuCount = int.Parse(Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS"));
				}
				catch { m_cpuCount = -1; }
			}
			
			if (m_cpuCount < 1)
			{
				m_cpuCount = 1;
			}
			
			m_cpuUse = m_cpuCount;			
			
		}

		/// <summary>
		/// Gets/sets the listening port for the server.
		/// </summary>
		public ushort Port
		{
			get { return _port; }
			set { _port = value; }
		}

		/// <summary>
		/// Gets/sets the listening address for the server.
		/// </summary>
		public IPAddress IP
		{
			get { return _ip; }
			set { _ip = value; }
		}
		
		/// <summary>
		/// Gets or sets the root directory of the server
		/// </summary>
		public string RootDirectory
		{
			get { return m_rootDirectory; }
			set { m_rootDirectory = value; }
		}

		/// <summary>
		/// Gets or sets the log configuration file of this server
		/// </summary>
		public string LogConfigFile
		{
			get
			{
				if(Path.IsPathRooted(m_logConfigFile))
					return m_logConfigFile;
				else
					return Path.Combine(m_rootDirectory, m_logConfigFile);
			}
			set { m_logConfigFile = value; }
		}
		
		/// <summary>
		/// Gets or sets the auto account creation flag
		/// </summary>
		public bool AutoAccountCreation
		{
			get { return m_autoAccountCreation; }
			set { m_autoAccountCreation = value; }
		}		
		
		public int CpuCount
		{
			get { return m_cpuCount; }
			set { m_cpuCount = value; }
		}
		
		public int CPUUse
		{
			get { return m_cpuUse; }
			set { m_cpuUse = value; }
		}
		
		public int MaxClientCount
		{
			get { return m_maxClientCount; }
			set { m_maxClientCount = value; }
		}
		
		/// <summary>
		/// Gets or sets the xml database path
		/// </summary>
		public string DBConnectionString
		{
			get { return m_dbConnectionString; }
			set { m_dbConnectionString = value; }
		}

		/// <summary>
		/// Gets or sets the DB type
		/// </summary>
		public ConnectionType DBType
		{
			get { return m_dbType; }
			set { m_dbType = value; }
		}

		/// <summary>
		/// Gets or sets the autosave flag
		/// </summary>
		public bool AutoSave
		{
			get { return m_autoSave; }
			set { m_autoSave = value; }
		}

		/// <summary>
		/// Gets or sets the autosave interval
		/// </summary>
		public int SaveInterval
		{
			get { return m_saveInterval; }
			set { m_saveInterval = value; }
		}
		
		protected eGameServerType m_serverType;
		public eGameServerType ServerType
		{
			get { return m_serverType; }
			set { m_serverType = value; }
		}
		
		/// <summary>
		/// Loads the configuration values from the given configuration element.
		/// </summary>
		/// <param name="root">the root config element</param>
		protected virtual void LoadFromConfig(ConfigElement root)
		{
			string ip = root["Server"]["IP"].GetString("any");
			_ip = ip == "any" ? IPAddress.Any : IPAddress.Parse(ip);
			_port = (ushort) root["Server"]["Port"].GetInt(_port);
			
			m_logConfigFile = root["Server"]["LogConfigFile"].GetString(m_logConfigFile);
			m_autoAccountCreation = root["Server"]["AutoAccountCreation"].GetBoolean(m_autoAccountCreation);
			
			string db = root["Server"]["DBType"].GetString("mysql");
			switch (db.ToLower())
			{
				case "mysql":
					m_dbType = ConnectionType.DATABASE_MYSQL;
					break;
				case "sqlite":
					m_dbType = ConnectionType.DATABASE_SQLITE;
					break;
				case "mssql":
					m_dbType = ConnectionType.DATABASE_MSSQL;
					break;
				case "odbc":
					m_dbType = ConnectionType.DATABASE_ODBC;
					break;
				default:
					m_dbType = ConnectionType.DATABASE_SQLITE;
					break;
			}
			
			string serverType = root["Server"]["GameType"].GetString("Normal");
			switch (serverType.ToLower())
			{
				case "normal":
					m_serverType = eGameServerType.GST_Normal;
					break;
				case "casual":
					m_serverType = eGameServerType.GST_Casual;
					break;
			}

			m_dbConnectionString = root["Server"]["DBConnectionString"].GetString(m_dbConnectionString);
			m_autoSave = root["Server"]["DBAutosave"].GetBoolean(m_autoSave);
			m_saveInterval = root["Server"]["DBAutosaveInterval"].GetInt(m_saveInterval);
			m_maxClientCount = root["Server"]["MaxClientCount"].GetInt(m_maxClientCount);
			m_cpuCount = root["Server"]["CpuCount"].GetInt(m_cpuCount);
			
			if (m_cpuCount < 1)
				m_cpuCount = 1;
			
			m_cpuUse = root["Server"]["CpuUse"].GetInt(m_cpuUse);
			if (m_cpuUse < 1)
				m_cpuUse = 1; 			
		}

		/// <summary>
		/// Load the configuration from an XML source file.
		/// </summary>
		/// <param name="configFile">the file to load from</param>
		public void LoadFromXMLFile(FileInfo configFile)
		{
			if (configFile == null)
				throw new ArgumentNullException("configFile");

			XMLConfigFile xmlConfig = XMLConfigFile.ParseXMLFile(configFile);
			LoadFromConfig(xmlConfig);
		}

		/// <summary>
		/// Saves the values to the given configuration element.
		/// </summary>
		/// <param name="root">the configuration element to save to</param>
		protected virtual void SaveToConfig(ConfigElement root)
		{
			root["Server"]["Port"].Set(_port);
			root["Server"]["IP"].Set(_ip);
			root["Server"]["LogConfigFile"].Set(m_logConfigFile);
			root["Server"]["AutoAccountCreation"].Set(m_autoAccountCreation);

			string db = "SQLITE";
			
			switch (m_dbType)
			{
				case ConnectionType.DATABASE_MYSQL:
					db = "MYSQL";
					break;
				case ConnectionType.DATABASE_SQLITE:
					db = "SQLITE";
					break;
				case ConnectionType.DATABASE_MSSQL:
					db = "MSSQL";
					break;
				case ConnectionType.DATABASE_ODBC:
					db = "ODBC";
					break;
				default:
					m_dbType = ConnectionType.DATABASE_SQLITE;
					break;
			}
			root["Server"]["DBType"].Set(db);
			root["Server"]["DBConnectionString"].Set(m_dbConnectionString);
			root["Server"]["DBAutosave"].Set(m_autoSave);
			root["Server"]["DBAutosaveInterval"].Set(m_saveInterval);
			root["Server"]["CpuUse"].Set(m_cpuUse);			
		}

		/// <summary>
		/// Saves the values to the given XML configuration file.
		/// </summary>
		/// <param name="configFile">the file to save to</param>
		public void SaveToXMLFile(FileInfo configFile)
		{
			if (configFile == null)
				throw new ArgumentNullException("configFile");

			var config = new XMLConfigFile();
			SaveToConfig(config);

			config.Save(configFile);
		}
	}
}
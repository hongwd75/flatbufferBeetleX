using Logic.database.attribute;

namespace Logic.database.table
{
	[DataTable(TableName = "Regions")]
    public class DBRegions : DataObject
    {
        /// <summary>
        /// The region id.
        /// </summary>
        private ushort m_regionID;

        /// <summary>
        /// The region name.
        /// </summary>
        private string m_name;

        /// <summary>
        /// The region description.
        /// </summary>
        private string m_description;

        /// <summary>
        /// The region ip.
        /// </summary>
        private string m_ip;

        /// <summary>
        /// The region port.
        /// </summary>
        private ushort m_port;

        /// <summary>
        /// The region expansion.
        /// </summary>
        private int m_expansion;

        /// <summary>
        /// The region housing flag.
        /// </summary>
        private bool m_housingEnabled;

        /// <summary>
        /// The region diving flag.
        /// </summary>
        private bool m_divingEnabled;

        /// <summary>
        /// The region water level.
        /// </summary>
        private int m_waterLevel;

		/// <summary>
		/// The class of this region
		/// </summary>
		private string m_classType;

		/// <summary>
		/// Should this region be treated as the Frontiers?
		/// </summary>
		private bool m_isFrontier;

        public DBRegions()
        {
            m_regionID = 0;
            m_name = string.Empty;
            m_description = string.Empty;
            m_ip = "127.0.0.1";
            m_port = 10400;
            m_expansion = 0;
            m_housingEnabled = false;
            m_divingEnabled = false;
            m_waterLevel = 0;
			m_classType = string.Empty;
			m_isFrontier = false;
        }

        /// <summary>
        /// Gets or sets the region id.
        /// </summary>
        [PrimaryKey]
        public ushort RegionID
        {
            get => m_regionID;
            set => SetProperty(ref m_regionID, value);             
        }

        /// <summary>
        /// Gets or sets the region name.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string Name
        {
	        get => m_name;
	        set => SetProperty(ref m_name, value);
        }

        /// <summary>
        /// Gets or sets the region description.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string Description
        {
	        get => m_description;
	        set => SetProperty(ref m_description, value);	        
        }

        /// <summary>
        /// Gets or sets the region ip.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string IP
        {
	        get => m_ip;
	        set => SetProperty(ref m_ip, value);
        }

        /// <summary>
        /// Gets or sets the region port.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public ushort Port
        {
	        get => m_port;
	        set => SetProperty(ref m_port, value);	        
        }

        /// <summary>
        /// Gets or sets the region expansion.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int Expansion
        {
	        get => m_expansion;
	        set => SetProperty(ref m_expansion, value);	      	        
        }

        /// <summary>
        /// Gets or sets the region housing flag.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public bool HousingEnabled
        {
	        get => m_housingEnabled;
	        set => SetProperty(ref m_housingEnabled, value);	        
        }

        /// <summary>
        /// Gets or sets the region diving flag.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public bool DivingEnabled
        {
	        get => m_divingEnabled;
	        set => SetProperty(ref m_divingEnabled, value);	  	        
        }

        /// <summary>
        /// Gets or sets the region water level.
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int WaterLevel
        {
	        get => m_waterLevel;
	        set => SetProperty(ref m_waterLevel, value);	        
        }

		/// <summary>
		/// Gets or sets the region class.
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 200)]
		public string ClassType
		{
			get => m_classType;
			set => SetProperty(ref m_classType, value);			
		}

		/// <summary>
		/// Should the keep manager manage keeps in this region?
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public bool IsFrontier
		{
			get => m_isFrontier;
			set => SetProperty(ref m_isFrontier, value);			
		}
	}
}
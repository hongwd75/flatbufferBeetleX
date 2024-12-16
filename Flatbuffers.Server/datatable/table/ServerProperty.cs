using Logic.database.attribute;

namespace Logic.database.table
{
    [DataTable(TableName = "ServerProperty")]
    public class ServerProperty : DataObject
    {
        private string m_category;
        private string m_key;
        private string m_description;
        private string m_defaultValue;
        private string m_value;

        public ServerProperty()
        {
            m_category = string.Empty;
            m_key = string.Empty;
            m_description = string.Empty;
            m_defaultValue = string.Empty; ;
            m_value = string.Empty;
        }
        
        [DataElement(AllowDbNull = false)]
        public string Category
        {
            get => m_category;
            set => SetProperty(ref m_category, value);
        }        

        [PrimaryKey]
        public string Key
        {
            get => m_key;
            set => SetProperty(ref m_key, value);
        }

        [DataElement(AllowDbNull = false)]
        public string Description
        {
            get => m_description;
            set => SetProperty(ref m_description, value);
        }

        [DataElement(AllowDbNull = false)]
        public string DefaultValue
        {
            get => m_defaultValue;
            set => SetProperty(ref m_defaultValue, value);
        }

        [DataElement(AllowDbNull = false)]
        public string Value
        {
            get => m_value;
            set => SetProperty(ref m_value, value);
        }      
    }
}
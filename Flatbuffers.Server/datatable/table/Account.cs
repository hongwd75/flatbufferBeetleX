using Logic.database.attribute;

namespace Logic.database.table
{
    [DataTable(TableName="Account")]
    public class Account : DataObject
    {
        private string m_name;
        private string m_password;
        private DateTime m_creationDate;
        private DateTime m_lastLogin;
        private int m_realm;


        [PrimaryKey]
        public string Name
        {
            get => m_name;
            set => SetProperty(ref m_name, value); 
        }

        [DataElement(AllowDbNull = false)]
        public string Password
        {
            get => m_password;
            set => SetProperty(ref m_password, value);
        }
        
        [DataElement(AllowDbNull=false)]
        public DateTime CreationDate
        {
            get => m_creationDate;
            set => SetProperty(ref m_creationDate, value);
        }
        
        [DataElement(AllowDbNull=true)]
        public DateTime LastLogin
        {
            get => m_lastLogin;
            set => SetProperty(ref m_lastLogin, value);
        }        
     
        [DataElement(AllowDbNull=false)]
        public int Realm
        {
            get => m_realm;
            set => SetProperty(ref m_realm, value);            
        }        
    }
}
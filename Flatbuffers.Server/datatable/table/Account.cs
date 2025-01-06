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
        private uint m_plvl;
        private bool m_isMuted;
        
        public Account()
        {
            m_name = null;
            m_password = null;
            m_creationDate = DateTime.Now;
            m_plvl = 1;
            m_realm = 0;
            m_isMuted = false;
        }
        
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
        
        /// <summary>
        /// The private level of this account (admin=3, GM=2 or player=1)
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public uint PrivLevel
        {
            get => m_plvl;
            set => SetProperty(ref m_plvl, value);            
        }
        
        /// <summary>
        /// Is this account muted from public channels?
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public bool IsMuted
        {
            get => m_isMuted;
            set => SetProperty(ref m_isMuted, value);             
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
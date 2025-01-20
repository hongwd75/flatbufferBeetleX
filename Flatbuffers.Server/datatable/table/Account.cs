using Logic.database.attribute;

namespace Logic.database.table
{
    [DataTable(TableName="Account")]
    public class Account : DataObject
    {
        private string m_name;
        private string m_password;
        private string m_language;
        private string m_guildid;
        private DateTime m_creationDate;
        private DateTime m_lastLogin;
        private int m_realm;
        private uint m_plvl;
        private int m_state;
        private bool m_isMuted;
        private String m_mail;
        private String m_nickname;
        private bool m_isAnonymous;
        public Account()
        {
            m_name = null;
            m_password = null;
            m_creationDate = DateTime.Now;
            m_plvl = 1;
            m_realm = 0;
            m_state = 1;
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
        /// ID of the guild this character is in
        /// </summary>
        [DataElement(AllowDbNull = true, Index = true)]
        public string GuildID
        {
            get => m_guildid;
            set { Dirty = true; m_guildid = value.ToUpper(); }            
        }
        
        [DataElement(AllowDbNull = false)]
        public string Nickname
        {
            get => m_nickname;
            set => SetProperty(ref m_nickname, value);     
        }
        
        [DataElement(AllowDbNull = false)]
        public string Language
        {
            get => m_language;
            set { Dirty = true; m_language = value.ToUpper(); }
        }
        
        [DataElement(AllowDbNull = false)]
        public bool IsAnonymous
        {
            get => m_isAnonymous;
            set => SetProperty(ref m_isAnonymous, value);            
        }
        
        [DataElement(AllowDbNull = false)]
        public uint PrivLevel
        {
            get => m_plvl;
            set => SetProperty(ref m_plvl, value);            
        }
        
        /// <summary>
        /// 0 이상이 되어야 정상 플레이어
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public int Status {
            get => m_state;
            set => SetProperty(ref m_state, value);                   
        }

        [DataElement(AllowDbNull = true)]
        public string Mail {
            get => m_mail;
            set => SetProperty(ref m_mail, value);            
        }
        
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
        
        [Relation(LocalField = nameof( Name ), RemoteField = nameof( DOLCharacters.AccountName ), AutoLoad = true, AutoDelete=true)]
        public DOLCharacters[] Characters;       
        
        [Relation(LocalField = nameof( Name ), RemoteField = nameof( DBBannedAccount.Account ), AutoLoad = true, AutoDelete = true)]
        public DBBannedAccount[] BannedAccount;
		
        /// <summary>
        /// List of Custom Params for this account
        /// </summary>
        [Relation(LocalField = nameof( Name ), RemoteField = nameof( AccountXCustomParam.Name ), AutoLoad = true, AutoDelete = true)]
        public AccountXCustomParam[] CustomParams;        
    }
    
    [DataTable(TableName = "AccountXCustomParam")]
    public class AccountXCustomParam : CustomParam
    {
        private string m_name;
        [DataElement(AllowDbNull = false, Index = true, Varchar = 255)]
        public string Name {
            get { return m_name; }
            set { Dirty = true; m_name = value; }
        }
        public AccountXCustomParam(string Name, string KeyName, string Value)
            : base(KeyName, Value)
        {
            this.Name = Name;
        }
		
        public AccountXCustomParam()
        {
        }
    }    
}
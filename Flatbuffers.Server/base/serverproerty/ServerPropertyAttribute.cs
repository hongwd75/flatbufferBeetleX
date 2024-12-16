namespace Game.Logic.ServerProperties
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ServerPropertyAttribute : Attribute
    {
        private string m_category;
        private string m_key;
        private string m_description;
        private object m_defaultValue;
        
        public ServerPropertyAttribute(string category, string key, string description, object defaultValue)
        {
            m_category = category;
            m_key = key;
            m_description = description;
            m_defaultValue = defaultValue;
        }
        
        public string Category { get => m_category; }
        public string Key { get => m_key; }
        public string Description { get => m_description; }
        public object DefaultValue { get => m_defaultValue; }        
    }
}
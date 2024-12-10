using Game.Logic.World;

namespace Game.Logic
{
    public class GameObject
    {
        public enum eObjectState : byte
        {
            Active,
            Inactive,
            Deleted
        }
        
        protected eRealm m_Realm;
        protected int m_ObjectID;
        protected volatile eObjectState m_ObjectState;
        
        // GET / SET ========================================================================
        public virtual eRealm Realm
        {
            get => m_Realm;
            set
            {
                m_Realm = value;
            }
        }
        public virtual int ObjectID
        {
            get => m_ObjectID;
            set
            {
                m_ObjectID = value;
            } 
        }
        public virtual eObjectState ObjectState
        {
            get => m_ObjectState;
            set
            {
                m_ObjectState = value;
            }
        }
        
    }
}
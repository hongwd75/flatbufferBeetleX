using Game.Logic.Geometry;
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
        
        // =============================================================================================
        #region 변수들
        protected eRealm m_Realm;
        protected int m_ObjectID;
        protected volatile eObjectState m_ObjectState;
        protected Region mCurrentRegion;
        protected string mName;
        #endregion

        // =============================================================================================
        #region GET / SET
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
        
        public Position Position { get; set; }
        public Coordinate Coordinate => Position.Coordinate;

        public string Name
        {
            get => mName;
            set
            {
                mName = value;
            }
        }
        #endregion        
    }
}
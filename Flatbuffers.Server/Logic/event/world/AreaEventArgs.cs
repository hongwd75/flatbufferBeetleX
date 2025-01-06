using Game.Logic.World;

namespace Game.Logic.Events;

public class AreaEventArgs : EventArgs
{
    IArea m_area;
    GameObject m_object;

    public AreaEventArgs(IArea area, GameObject obj)
    {
        m_area = area;
        m_object = obj;
    }
    
    public IArea Area
    {
        get {return m_area;}
    }
    
    public GameObject GameObject
    {
        get{return m_object;}
    }
}
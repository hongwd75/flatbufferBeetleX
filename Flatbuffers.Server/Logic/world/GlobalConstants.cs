namespace Game.Logic.World
{
    public enum eRealm
    {
        None = 0,
        Albion = 1,
        Midgard = 2,
        Hibernia = 3,
        _Last = 4,
    };
    
    public enum eGameServerStatus
    {
        GSS_Open = 0,
        GSS_Closed,
        GSS_Down,
        GSS_Full,
        GSS_Unknown
    }    
}
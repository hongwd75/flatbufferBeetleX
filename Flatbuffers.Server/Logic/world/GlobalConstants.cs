namespace Game.Logic.World
{
    public enum eRealm
    {
        None = 0,
        Albion = 1,
        Midgard = 2,
        Hibernia = 3
    };
    
    public enum eGameServerStatus
    {
        /// <summary>
        /// Server is open for connections
        /// </summary>
        GSS_Open = 0,
        /// <summary>
        /// Server is closed and won't accept connections
        /// </summary>
        GSS_Closed,
        /// <summary>
        /// Server is down
        /// </summary>
        GSS_Down,
        /// <summary>
        /// Server is full, no more connections accepted
        /// </summary>
        GSS_Full,
        /// <summary>
        /// Unknown server status
        /// </summary>
        GSS_Unknown
    }    
}
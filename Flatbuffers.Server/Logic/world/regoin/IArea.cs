using Game.Logic.Events;
using Game.Logic.Geometry;

namespace Game.Logic.World;

public interface IArea : ITranslatableObject
{					
    /// <summary>
    /// Returns the ID of this zone
    /// </summary>
    ushort ID{ get; set;}		

    void UnRegisterPlayerEnter(GameEventHandler callback);
    void UnRegisterPlayerLeave(GameEventHandler callback);
    void RegisterPlayerEnter(GameEventHandler callback);
    void RegisterPlayerLeave(GameEventHandler callback);

    /// <summary>
    /// Checks wether is intersects with given zone.
    /// This is needed to build an area.zone mapping cache for performance.		
    /// </summary>
    /// <param name="zone"></param>
    /// <returns></returns>
    bool IsIntersectingZone(Zone zone);

    bool IsContaining(Coordinate spot, bool ignoreZ = false);

    [Obsolete("Use .IsContaining(Coordinate[,bool]) instead!")]
    bool IsContaining(IPoint3D spot);

    [Obsolete("Use .IsContaining(Coordinate[,bool]) instead!")]
    bool IsContaining(IPoint3D spot, bool checkZ);

    [Obsolete("Use .IsContaining(Coordinate[,bool]) instead!")]
    bool IsContaining(int x, int y, int z);

    [Obsolete("Use .IsContaining(Coordinate[,bool]) instead!")]
    bool IsContaining(int x, int y, int z, bool checkZ);
		
    /// <summary>
    /// Called whenever a player leaves the given area
    /// </summary>
    /// <param name="player"></param>
    void OnPlayerLeave(GamePlayer player);

    /// <summary>
    /// Called whenever a player enters the given area
    /// </summary>
    /// <param name="player"></param>
    void OnPlayerEnter(GamePlayer player);
}
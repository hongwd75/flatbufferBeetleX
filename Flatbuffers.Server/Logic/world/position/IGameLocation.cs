using Game.Logic.Geometry;

namespace Game.Logic.World;

public interface IGameLocation
{
    int X { get; }
    int Y { get; }
    int Z { get; }
    Position Position { get; }
    ushort RegionID { get; }
    ushort Heading { get; }
    string Name { get; }
}
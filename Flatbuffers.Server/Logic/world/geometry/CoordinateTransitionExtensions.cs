using Game.Logic.World;

namespace Game.Logic.Geometry;

public static class CoordinateTransitionExtensions
{
    public static Point3D ToPoint3D(this Coordinate coordinate)
        => new Point3D(coordinate.X, coordinate.Y, coordinate.Z);

    public static Coordinate ToCoordinate(this IPoint3D point)
        => Coordinate.Create(point.X, point.Y, point.Z);
}
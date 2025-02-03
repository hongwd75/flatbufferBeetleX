using Game.Logic.Geometry;
using Logic.database.table;

namespace Game.Logic.World.Movement;

public class PathPoint
{
    public Coordinate Coordinate { get; set; }
    
    public PathPoint(Coordinate coordinate, int maxspeed, ePathType type)
    {
        Coordinate = coordinate;
        MaxSpeed = maxspeed;
        Type = type;
    }

    public PathPoint(int x, int y, int z, int maxspeed, ePathType type)
        : this(Coordinate.Create(x, y, z), maxspeed, type) { }

    public PathPoint(DBPathPoint dbEntry, ePathType type)
    {
        Coordinate = Coordinate.Create(dbEntry.X, dbEntry.Y, dbEntry.Z);
        MaxSpeed = dbEntry.MaxSpeed;
        WaitTime = dbEntry.WaitTime;
        Type = type;
    }

    public Angle AngleToNextPathPoint => Coordinate.GetOrientationTo(Next.Coordinate);

    public int MaxSpeed { get; set; }

    public PathPoint Prev { get; set; }
    public PathPoint Next { get; set; }

    public bool FiredFlag { get; set; } = false;

    public ePathType Type { get; set; }

    public int WaitTime { get; set; } = 0;

    public DBPathPoint GenerateDbEntry()
    {
        var dbPathPoint = new DBPathPoint();
        dbPathPoint.X = Coordinate.X;
        dbPathPoint.Y = Coordinate.Y;
        dbPathPoint.Z = Coordinate.Z;
        dbPathPoint.MaxSpeed = MaxSpeed;
        dbPathPoint.WaitTime = WaitTime;
        return dbPathPoint;
    }
}
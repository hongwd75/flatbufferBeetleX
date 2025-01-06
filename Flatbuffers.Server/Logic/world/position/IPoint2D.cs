namespace Game.Logic.World;

public interface IPoint2D
{
    int X { get; set; }
    int Y { get; set; }

    ushort GetHeading(IPoint2D point);

    Point2D GetPointFromHeading(ushort heading, int distance);

    int GetDistance(IPoint2D point);

    void Clear();    
}
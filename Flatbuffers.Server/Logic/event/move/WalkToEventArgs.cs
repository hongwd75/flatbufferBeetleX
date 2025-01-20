using Game.Logic.Geometry;

namespace Game.Logic.Events;

public class WalkToEventArgs : EventArgs
{
    public WalkToEventArgs(Coordinate target, int speed)
    {
        Target = target;
        Speed = speed;
    }

    /// <summary>
    /// The spot to walk to.
    /// </summary>
    public Coordinate Target { get; private set; }

    /// <summary>
    /// The speed to walk at.
    /// </summary>
    public int Speed { get; private set; }
}
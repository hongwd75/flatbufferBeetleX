namespace Game.Logic.Events;

public class MoveToEventArgs : EventArgs
{
    private ushort regionID;
    private int x;
    private int y;
    private int z;
    private ushort heading;

    public MoveToEventArgs(ushort regionId, int x, int y, int z, ushort heading)
    {
        this.regionID = regionId;
        this.x = x;
        this.y = y;
        this.z = z;
        this.heading = heading;
    }
    public ushort RegionId
    {
        get { return regionID; }
    }
    public int X
    {
        get { return x; }
    }
    public int Y
    {
        get { return y; }
    }
    public int Z
    {
        get { return z; }
    }
    public ushort Heading
    {
        get { return heading; }
    }
}
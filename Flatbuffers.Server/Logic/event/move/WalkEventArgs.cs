namespace Game.Logic.Events;

public class WalkEventArgs : EventArgs
{
    private int speed;
    public WalkEventArgs(int speed)
    {
        this.speed=speed;
    }
    public int Speed
    {
        get { return speed; }
    }
}
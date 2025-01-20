namespace Game.Logic.Events;

public class TimerEventArgs : SourceEventArgs
{		
    private string timerId;
    public TimerEventArgs(GameLiving source, string timerId) : base (source)
    {
        this.timerId = timerId;
    }
    public string TimerID
    {
        get { return timerId; }
    }
}
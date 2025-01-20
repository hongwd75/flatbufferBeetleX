namespace Game.Logic.Events;

public class SayReceiveEventArgs : SourceEventArgs
{
    private GameLiving target;
    private string text;

    public SayReceiveEventArgs(GameLiving source, GameLiving target,  string text) : base(source)
    {			
        this.target = target;
        this.text = text;
    }		
    public GameLiving Target
    {
        get { return target; }
    }
    public string Text
    {
        get { return text; }
    }
}
namespace Game.Logic.Events;

public class InteractEventArgs : SourceEventArgs 
{
    public InteractEventArgs(GamePlayer source) : base(source)
    {			
    }

    public new GamePlayer Source
    {
        get {return (GamePlayer) base.Source;}
    }    
}
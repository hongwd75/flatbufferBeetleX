namespace Game.Logic.Events;

public class YellEventArgs : SayEventArgs
{
    public YellEventArgs(string text) : base(text)
    {
    }
}
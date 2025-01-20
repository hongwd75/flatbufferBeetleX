namespace Game.Logic.Events;

public class YellReceiveEventArgs : SayReceiveEventArgs
{
    public YellReceiveEventArgs(GameLiving source, GameLiving target, string text) : base(source, target, text)
    {
    }
}
namespace Game.Logic.Events;

public class WhisperReceiveEventArgs : SayReceiveEventArgs
{
    public WhisperReceiveEventArgs(GameLiving source, GameLiving target, string text) : base(source, target, text)
    {
    }
}
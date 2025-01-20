namespace Game.Logic.Events;

public class WhisperEventArgs : SayEventArgs
{
    private GameObject target;
    public WhisperEventArgs(GameObject target, string text) : base(text)
    {
        this.target = target;
    }
    public GameObject Target
    {
        get { return target; }
    }
}
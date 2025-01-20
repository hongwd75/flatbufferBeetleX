namespace Game.Logic.Events;

public class SayEventArgs : EventArgs
{
    private string text;

    public SayEventArgs(string text)
    {
        this.text = text;
    }
    public string Text
    {
        get { return text; }
    }
}
using Game.Logic.World;

namespace Game.Logic.Events;

public class AreaEvent : GameEvent
{
    public AreaEvent(string name) : base(name)
    {			
    }	

    public override bool IsValidFor(object o)
    {
        return o is IArea;
    }

    public static readonly AreaEvent PlayerEnter = new AreaEvent("AreaEvent.PlayerEnter");
    public static readonly AreaEvent PlayerLeave = new AreaEvent("AreaEvent.PlayerLeave");
}
using Game.Logic.CharacterClasses;

namespace Game.Logic.Events.guild;

public class PlayerPromotedEventArgs : EventArgs
{
    private GamePlayer player;
    private CharacterClass oldClass;
    public PlayerPromotedEventArgs(GamePlayer player, CharacterClass oldClass)
    {
        this.player = player;
        this.oldClass = oldClass;
    }
    public GamePlayer Player
    {
        get { return player; }
    }
    public CharacterClass OldClass
    {
        get { return oldClass; }
    }
}
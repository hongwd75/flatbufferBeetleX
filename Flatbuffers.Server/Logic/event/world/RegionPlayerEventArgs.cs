namespace Game.Logic.Events;

public class RegionPlayerEventArgs : EventArgs
{
    /// <summary>
    /// The player who enter or leave region
    /// </summary>
    private GamePlayer m_player;

    /// <summary>
    /// Constructs a new player enter or leave region event args
    /// </summary>
    public RegionPlayerEventArgs(GamePlayer player)
    {
        this.m_player=player;
    }

    /// <summary>
    /// Gets the player
    /// </summary>
    public GamePlayer Player
    {
        get { return m_player; }
    }
}
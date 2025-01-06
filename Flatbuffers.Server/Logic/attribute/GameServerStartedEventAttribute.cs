namespace Game.Logic.attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
public class GameServerStartedEventAttribute : Attribute
{
    /// <summary>
    /// Constructs a new GameServerStartedEventAttribute
    /// </summary>
    public GameServerStartedEventAttribute()
    {
    }
}
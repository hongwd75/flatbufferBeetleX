namespace Game.Logic.attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
public class ScriptLoadedEventAttribute : Attribute
{
    /// <summary>
    /// Constructs a new ScriptLoadedEventAttribute
    /// </summary>
    public ScriptLoadedEventAttribute()
    {
    }
}
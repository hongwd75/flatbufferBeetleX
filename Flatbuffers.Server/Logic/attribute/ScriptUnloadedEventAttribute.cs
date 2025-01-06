namespace Game.Logic.attribute;

[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
public class ScriptUnloadedEventAttribute : Attribute
{
    /// <summary>
    /// Constructs a new ScriptUnloadedEventAttribute
    /// </summary>
    public ScriptUnloadedEventAttribute()
    {
    }
}
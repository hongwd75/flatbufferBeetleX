namespace Game.Logic.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RefreshCommandAttribute : Attribute
{
    public RefreshCommandAttribute()
    {
    }
}
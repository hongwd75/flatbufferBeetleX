namespace Game.Logic.attribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public class GameServerStoppedEventAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new GameServerStoppedEventAttribute
        /// </summary>
        public GameServerStoppedEventAttribute()
        {
        }
    }
}
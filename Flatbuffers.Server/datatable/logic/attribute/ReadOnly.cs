namespace Logic.database.attribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ReadOnly : Attribute
    {
        /// <summary>
        /// Constructor for Attribute
        /// </summary>
        public ReadOnly()
        {
        }
    }
}
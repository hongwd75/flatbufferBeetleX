namespace Logic.database.attribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PrimaryKey : Attribute
    {
        /// <summary>
        /// Constructor for Attribute
        /// </summary>
        public PrimaryKey()
        {
            AutoIncrement = false;
        }

        /// <summary>
        /// Indicates if this column will auto increment.
        /// Setting to true will eliminate the tablename_id column for this table.
        /// </summary>
        public bool AutoIncrement { get; set; }
    }
}
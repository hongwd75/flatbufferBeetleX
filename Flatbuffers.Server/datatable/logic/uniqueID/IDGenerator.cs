namespace Logic.database.uniqueID
{
    public static class IDGenerator
    {
        /// <summary>
        /// Generate a new GUID String
        /// </summary>
        /// <returns>a new unique Key</returns>
        public static string GenerateID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
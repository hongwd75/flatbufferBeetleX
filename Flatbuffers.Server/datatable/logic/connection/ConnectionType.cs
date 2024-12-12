namespace Logic.database.connection
{
    public enum ConnectionType
    {
        /// <summary>
        /// Use the internal MySQL-Driver for Database
        /// </summary>
        DATABASE_MYSQL,
        /// <summary>
        /// Use the internal SQLite-Driver for Database
        /// </summary>
        DATABASE_SQLITE,
        /// <summary>
        /// Use Microsoft SQL-Server
        /// </summary>
        DATABASE_MSSQL,
        /// <summary>
        /// Use an ODBC-Datasource
        /// </summary>
        DATABASE_ODBC
    }
}
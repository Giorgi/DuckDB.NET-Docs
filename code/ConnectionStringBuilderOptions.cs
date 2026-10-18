using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ConnectionStringBuilderOptions
{
    public static void Run()
    {
        #region Example
        var builder = new DuckDBConnectionStringBuilder
        {
            DataSource = "train_services.db",
            AccessMode = DuckDBAccessMode.ReadOnly,
            Threads = 8,
            MemoryLimit = "10GB"
        };

        // An option without a typed property is set through the indexer.
        builder["TimeZone"] = "UTC";

        // DataSource=train_services.db;access_mode=read_only;threads=8;memory_limit=10GB;TimeZone=UTC
        using var connection = new DuckDBConnection(builder.ConnectionString);
        connection.Open();
        #endregion
    }
}

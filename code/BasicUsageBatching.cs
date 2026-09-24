using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class BasicUsageBatching
{
    public static void Run()
    {
        #region Example
        using var connection = new DuckDBConnection("DataSource=:memory:");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "INSTALL httpfs;LOAD httpfs;";
        command.ExecuteNonQuery();
        #endregion
    }
}

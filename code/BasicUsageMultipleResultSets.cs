using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class BasicUsageMultipleResultSets
{
    public static void Run()
    {
        #region Example
        using var connection = new DuckDBConnection("DataSource=:memory:");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM weather ORDER BY city, temp_lo;SELECT DISTINCT city FROM weather;";
        using var reader = command.ExecuteReader();

        do
        {
          //consume data from the reader
        } while (reader.NextResult());
        #endregion
    }
}

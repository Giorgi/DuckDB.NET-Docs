using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class StandardAppenderCreateRowFailedRow
{
    public static void Retry(DuckDBConnection connection)
    {
        CreateTable(connection);

        #region Retry
        // CREATE TABLE Readings(name VARCHAR, value INTEGER)
        var value = 42;

        using (var appender = connection.CreateAppender("Readings"))
        {
            var row = appender.CreateRow();

            try
            {
                row.AppendValue(DateTime.UtcNow); // throws: a DateTime cannot be written to a VARCHAR column
            }
            catch (InvalidOperationException)
            {
                row.AppendValue("fixed"); // the column is still unwritten, so write it again
            }

            row.AppendValue(value).EndRow(); // the row is complete and the appender stays usable
        } // Dispose -> Close writes ("fixed", 42)
        #endregion
    }

    public static void Skip(DuckDBConnection connection)
    {
        CreateTable(connection);

        #region Skip
        var value = 1;

        using (var appender = connection.CreateAppender("Readings"))
        {
            appender.CreateRow().AppendValue("completed").AppendValue(value).EndRow();

            try
            {
                appender.CreateRow().AppendValue(DateTime.UtcNow).AppendValue(value + 1).EndRow();
            }
            catch (InvalidOperationException)
            {
                // Moving on leaves the row incomplete.
            }

            // Throws: the incomplete row is discarded and no more rows may be appended.
            appender.CreateRow();
        } // Dispose -> Close writes the completed ("completed", 1) row
        #endregion
    }

    private static void CreateTable(DuckDBConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE Readings(name VARCHAR, value INTEGER)";
        command.ExecuteNonQuery();
    }
}

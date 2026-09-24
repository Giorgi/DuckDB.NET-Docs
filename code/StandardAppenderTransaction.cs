using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class StandardAppenderTransaction
{
    public static void Run(DuckDBConnection connection, int rows)
    {
        #region Example
        using var transaction = connection.BeginTransaction();
        try
        {
            using (var appender = connection.CreateAppender("AppenderTest"))
            {
                for (var i = 0; i < rows; i++)
                {
                    appender.AppendRow(i, static (row, value) => row.AppendValue(value).AppendValue(value + 2));
                }
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback(); // Nothing is persisted, including rows already flushed
            throw;
        }
        #endregion
    }
}

using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class StandardAppenderFailedRow
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        using (var appender = connection.CreateAppender("AppenderTest"))
        {
            appender.AppendRow(row => row.AppendValue((int?)1).AppendValue((int?)2)); // completed

            try
            {
                appender.AppendRow(row =>
                {
                    row.AppendValue((int?)3).AppendValue((int?)4);
                    throw new InvalidOperationException("something went wrong");
                });
            }
            catch (InvalidOperationException)
            {
                // The (3, 4) row is discarded and no more rows may be appended.
            }
        } // Dispose -> Close writes the completed (1, 2) row
        #endregion
    }
}

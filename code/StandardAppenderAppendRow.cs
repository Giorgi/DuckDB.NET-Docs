using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class StandardAppenderAppendRow
{
    public static void Run(DuckDBConnection connection, int rows)
    {
        #region Example
        using (var appender = connection.CreateAppender("AppenderTest"))
        {
            for (var i = 0; i < rows; i++)
            {
                appender.AppendRow(i, static (row, value) =>
                {
                    row.AppendValue(value).AppendValue(value + 2);
                });
            }
        }
        #endregion
    }
}

using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class StandardAppenderClear
{
    public static void Run(DuckDBAppender appender)
    {
        #region Example
        appender.CreateRow().AppendValue(1).AppendValue(10).EndRow();
        appender.Clear(); // Discards the row above
        appender.CreateRow().AppendValue(2).AppendValue(20).EndRow();
        appender.Close(); // Only (2, 20) is inserted
        #endregion
    }
}

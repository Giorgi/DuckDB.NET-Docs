using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class StandardAppenderClear
{
    public static void Run(DuckDBAppender appender)
    {
        #region Example
        appender.CreateRow().AppendValue((int?)1).AppendValue((int?)10).EndRow();
        appender.Clear(); // Discards the row above
        appender.CreateRow().AppendValue((int?)2).AppendValue((int?)20).EndRow();
        appender.Close(); // Only (2, 20) is inserted
        #endregion
    }
}

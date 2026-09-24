using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class MappedAppenderDispose
{
    public static void Run(DuckDBConnection connection, Person[] records)
    {
        #region Example
        using (var appender = connection.CreateAppender<Person, PersonMap>("people"))
        {
            appender.AppendRecords(records);
        } // Automatically disposed here
        #endregion
    }
}

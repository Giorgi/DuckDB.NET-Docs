using DuckDB.NET.Data;
using static Samples.Helpers;

namespace Samples.Snippets;

public static class TableFunctionAsync
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterTableFunction("ext_async",
            (int count) => FetchEmployeesAsync(count).ToBlockingEnumerable(),
            e => new { e.Id, e.Name });
        #endregion
    }
}

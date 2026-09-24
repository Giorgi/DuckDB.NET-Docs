using DuckDB.NET.Data;
using static Samples.Helpers;

namespace Samples.Snippets;

public static class TableFunctionCustomParameterName
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterTableFunction("employees",
            (int count, [Named("max_rows")] int? limit) => GetEmployees(limit ?? count),
            e => new { e.Id, e.Name });
        #endregion
    }
}

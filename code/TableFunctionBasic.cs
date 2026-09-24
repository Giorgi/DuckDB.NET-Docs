using DuckDB.NET.Data;
using static Samples.Helpers;

namespace Samples.Snippets;

public static class TableFunctionBasic
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterTableFunction("employees",
            (int count) => GetEmployees(count),
            e => new { e.Id, e.Name });
        #endregion
    }
}

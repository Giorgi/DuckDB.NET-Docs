using DuckDB.NET.Data;
using static Samples.Helpers;

namespace Samples.Snippets;

public static class TableFunctionNamedParameter
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterTableFunction("employees",
            (int count, [Named] string? prefix) =>
                GetEmployees(count).Select(e => e with { Name = (prefix ?? "") + e.Name }),
            e => new { e.Id, e.Name });
        #endregion
    }
}

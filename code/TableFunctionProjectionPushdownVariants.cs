using DuckDB.NET.Data;
using static Samples.Helpers;

namespace Samples.Snippets;

public static class TableFunctionProjectionPushdownVariants
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        // Zero SQL parameters - projection only
        connection.RegisterTableFunction("all_employees",
            (IReadOnlyList<ProjectedColumn> projected) => FetchAll(projected),
            e => new { e.Id, e.Name });

        // Mixed with named parameters
        connection.RegisterTableFunction("employees",
            (IReadOnlyList<ProjectedColumn> projected, int count, [Named] string? prefix) =>
                FetchEmployees(count, prefix, projected),
            e => new { e.Id, e.Name, e.Salary });
        #endregion
    }
}

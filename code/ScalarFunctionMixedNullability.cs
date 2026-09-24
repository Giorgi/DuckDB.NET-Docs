using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionMixedNullability
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction<int?, int, string>("coalesce_add",
            (a, b) => a.HasValue ? (a.Value + b).ToString() : b.ToString());
        #endregion
    }
}

using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionNullableReference
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction<string?, string>("echo_or_default",
            s => s ?? "was_null");
        #endregion
    }
}

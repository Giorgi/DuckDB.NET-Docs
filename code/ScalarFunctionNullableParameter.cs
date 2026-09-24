using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionNullableParameter
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction<int?, string>("describe_val",
            x => x.HasValue ? x.Value.ToString() : "nothing");
        #endregion
    }
}

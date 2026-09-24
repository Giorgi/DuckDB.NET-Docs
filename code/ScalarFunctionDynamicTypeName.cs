using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionDynamicTypeName
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction<object, string>("net_type_name",
            value => value.GetType().Name);
        #endregion
    }
}

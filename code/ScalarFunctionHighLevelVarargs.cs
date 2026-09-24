using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionHighLevelVarargs
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction("sum_all", (long[] args) => args.Sum());
        #endregion
    }
}

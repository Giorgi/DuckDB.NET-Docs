using DuckDB.NET.Data;
using static Samples.Helpers;

namespace Samples.Snippets;

public static class ScalarFunctionParameterCounts
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        // Zero parameters
        connection.RegisterScalarFunction("the_answer", () => 42);

        // One parameter
        connection.RegisterScalarFunction<int, bool>("is_prime", IsPrime);

        // Two parameters
        connection.RegisterScalarFunction<long, long, long>("add", (a, b) => a + b);

        // Three parameters
        connection.RegisterScalarFunction<int, int, int, int>("clamp",
            (value, min, max) => Math.Clamp(value, min, max));
        #endregion
    }
}

using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionNullHandling
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction<string, string>("echo_nullable", (readers, writer, rowCount) =>
        {
            for (ulong i = 0; i < rowCount; i++)
            {
                var value = readers[0].GetValue<string>(i);
                writer.WriteValue(value ?? "was_null", i);
            }
        }, new() { HandlesNulls = true });
        #endregion
    }
}

using Dapper;
using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionNonPure
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction<long, long>("my_random_scalar", (readers, writer, rowCount) =>
        {
            for (ulong index = 0; index < rowCount; index++)
            {
                var value = Random.Shared.NextInt64(readers[0].GetValue<long>(index));

                writer.WriteValue(value, index);
            }
        }, new ScalarFunctionOptions { IsPureFunction = false });

        connection.Query<long>("SELECT my_random_scalar(i) FROM some_table");
        #endregion
    }
}

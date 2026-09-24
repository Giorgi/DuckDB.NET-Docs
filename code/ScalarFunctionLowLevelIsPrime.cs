using Dapper;
using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionLowLevelIsPrime
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction<int, bool>("is_prime", (readers, writer, rowCount) =>
        {
            for (ulong index = 0; index < rowCount; index++)
            {
                var prime = true;
                var value = readers[0].GetValue<int>(index);

                for (int i = 2; i <= Math.Sqrt(value); i++)
                {
                    if (value % i == 0)
                    {
                        prime = false;
                        break;
                    }
                }

                writer.WriteValue(prime, index);
            }
        });

        var primes = connection.Query<int>("SELECT i FROM range(2, 100) t(i) where is_prime(i::INT)").ToList();

        //primes will be 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97
        #endregion
    }
}

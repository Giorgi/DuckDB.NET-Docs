using System.Globalization;
using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ScalarFunctionDynamicTypes
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        connection.RegisterScalarFunction("format_net",
            (object value, string format) => value is IFormattable f
                ? f.ToString(format, CultureInfo.InvariantCulture)
                : value.ToString());
        #endregion
    }
}

using Apache.Arrow;
using Apache.Arrow.Types;
using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class ArrowAppender
{
    public static void Run()
    {
        #region Example
        using var connection = new DuckDBConnection("Data Source=:memory:");
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE weather(city VARCHAR, temp_lo INTEGER, temp_hi INTEGER);";
            command.ExecuteNonQuery();
        }

        var schema = new Schema.Builder()
            .Field(field => field.Name("city").DataType(StringType.Default))
            .Field(field => field.Name("temp_lo").DataType(Int32Type.Default))
            .Field(field => field.Name("temp_hi").DataType(Int32Type.Default))
            .Build();

        IArrowArray[] columns =
        [
            new StringArray.Builder().Append("San Francisco").Append("Hayward").Build(),
            new Int32Array.Builder().Append(46).Append(37).Build(),
            new Int32Array.Builder().Append(50).Append(54).Build(),
        ];

        using var batch = new RecordBatch(schema, columns, length: 2);

        using (var appender = connection.CreateArrowAppender("weather"))
        {
            appender.AppendBatch(batch);
        } // Dispose -> Close writes the two rows
        #endregion
    }
}

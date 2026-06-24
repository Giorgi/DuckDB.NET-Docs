using System.Linq;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using DuckDB.NET.Data;

using var duckDBConnection = new DuckDBConnection("Data Source=file.db");
duckDBConnection.Open();

using var command = duckDBConnection.CreateCommand();
command.CommandText = "SELECT city, temp_lo, temp_hi FROM weather ORDER BY city;";

// The caller owns the stream and must dispose it.
using IArrowArrayStream stream = command.ExecuteArrowStream();

Console.WriteLine("Columns: {0}", string.Join(", ", stream.Schema.FieldsList.Select(field => field.Name)));

while (await stream.ReadNextRecordBatchAsync() is { } batch)
{
    using (batch)
    {
        var city = (StringArray)batch.Column("city");
        var tempLo = (Int32Array)batch.Column("temp_lo");
        var tempHi = (Int32Array)batch.Column("temp_hi");

        for (var row = 0; row < batch.Length; row++)
        {
            Console.WriteLine("{0}: {1} to {2}", city.GetString(row), tempLo.GetValue(row), tempHi.GetValue(row));
        }
    }
}

using Apache.Arrow;
using DuckDB.NET.Data;

using var duckDBConnection = new DuckDBConnection("Data Source=file.db");
duckDBConnection.Open();

using var command = duckDBConnection.CreateCommand();
command.CommandText = "SELECT city, temp_lo, temp_hi FROM weather ORDER BY city;";

var rowCount = 0;

await foreach (RecordBatch batch in command.ExecuteArrowBatchesAsync())
{
    using (batch)
    {
        rowCount += batch.Length;
    }
}

Console.WriteLine("Read {0} rows from the result set.", rowCount);

using var duckDBConnection = new DuckDBConnection("Data Source=file.db");
duckDBConnection.Open();

using var command = duckDBConnection.CreateCommand();
command.CommandText = "Select count(*) from weather";
var count = command.ExecuteScalar();

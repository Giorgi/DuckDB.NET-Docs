using var connection = new DuckDBConnection("DataSource=:memory:");
connection.Open();

using var command = connection.CreateCommand();
command.CommandText = "INSTALL httpfs;LOAD httpfs;";
command.ExecuteNonQuery();

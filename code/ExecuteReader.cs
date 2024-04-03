using var duckDBConnection = new DuckDBConnection("Data Source=file.db");
duckDBConnection.Open();

using var command = duckDBConnection.CreateCommand();
command.CommandText = "SELECT city, (temp_hi + temp_lo) / 2 AS temp_avg, date FROM weather;";

using var reader = command.ExecuteReader();

while (reader.Read())
{
    Console.WriteLine("City: {0} Average Temperature: {1}, Date: {2}", 
					  reader.GetString(0), reader.GetDouble(1), reader.GetDateTime(2));
}
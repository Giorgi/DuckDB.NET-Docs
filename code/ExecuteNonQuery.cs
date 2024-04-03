using var duckDBConnection = new DuckDBConnection("Data Source=file.db");
duckDBConnection.Open();

using var command = duckDBConnection.CreateCommand();
command.CommandText = "CREATE TABLE weather (city VARCHAR, temp_lo INTEGER, temp_hi INTEGER, prcp REAL, date DATE);";
var executeNonQuery = command.ExecuteNonQuery();

command.CommandText = "INSERT INTO weather VALUES ('Tbilisi', 41, 55, 0.1, '2020-04-02');";
executeNonQuery = command.ExecuteNonQuery();

command.CommandText = "INSERT INTO weather VALUES ('San Francisco', 46, 50, 0.25, '1994-11-27');";
executeNonQuery = command.ExecuteNonQuery();

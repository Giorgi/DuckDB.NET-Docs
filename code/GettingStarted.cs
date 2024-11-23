using var duckDBConnection = new DuckDBConnection("Data Source=file.db");
duckDBConnection.Open();

using var command = duckDBConnection.CreateCommand();
command.CommandText = "CREATE TABLE integers(foo INTEGER, bar INTEGER);";
var executeNonQuery = command.ExecuteNonQuery();

command.CommandText = "INSERT INTO integers VALUES (3, 4), (5, 6), (7, NULL);";
executeNonQuery = command.ExecuteNonQuery();

command.CommandText = "Select count(*) from integers";
var count = command.ExecuteScalar();

command.CommandText = "SELECT foo, bar FROM integers";
var reader = command.ExecuteReader();

for (var index = 0; index < reader.FieldCount; index++)
{
    var column = reader.GetName(index);
    Console.Write($"{column} ");
}

Console.WriteLine();

while (reader.Read())
{
    for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
    {
        if (reader.IsDBNull(ordinal))
        {
            Console.WriteLine("NULL");
            continue;
        }

        var val = reader.GetFieldValue<int>(ordinal);
        Console.Write(val);
        Console.Write(" ");
    }
}

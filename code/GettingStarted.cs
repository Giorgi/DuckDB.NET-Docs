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
var queryResult = command.ExecuteReader();

for (var index = 0; index < queryResult.FieldCount; index++)
{
    var column = queryResult.GetName(index);
    Console.Write($"{column} ");
}

Console.WriteLine();

while (queryResult.Read())
{
    for (int ordinal = 0; ordinal < queryResult.FieldCount; ordinal++)
    {
        if (queryResult.IsDBNull(ordinal))
        {
            Console.WriteLine("NULL");
            continue;
        }

        var val = queryResult.GetValue(ordinal);
        Console.Write(val);
        Console.Write(" ");
    }
}

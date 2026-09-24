using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class BasicUsageParameterizedStatement
{
    public static void Run()
    {
        #region Example
        using var connection = new DuckDBConnection("DataSource=:memory:");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM person WHERE starts_with(name, $name_start_letter) AND age >= $minimum_age;";

        command.Parameters.Add(new DuckDBParameter("minimum_age", 40));
        command.Parameters.Add(new DuckDBParameter("name_start_letter", "B"));

        using var reader = command.ExecuteReader();
        #endregion
    }
}

using DuckDB.NET.Data;

namespace Samples.Snippets;

public static class BasicUsagePrepare
{
    public static void Run()
    {
        #region Example
        using var connection = new DuckDBConnection("DataSource=:memory:");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE weather (city VARCHAR, temp_lo INTEGER);";
        command.ExecuteNonQuery();

        var city = new DuckDBParameter { ParameterName = "city" };
        var tempLo = new DuckDBParameter { ParameterName = "temp_lo" };

        command.CommandText = "INSERT INTO weather VALUES ($city, $temp_lo);";
        command.Parameters.Add(city);
        command.Parameters.Add(tempLo);
        command.Prepare();

        foreach (var (name, temperature) in new[] { ("Tbilisi", 41), ("San Francisco", 46), ("Oslo", 28) })
        {
            city.Value = name;
            tempLo.Value = temperature;
            command.ExecuteNonQuery();
        }
        #endregion
    }
}

using DuckDB.NET.Data;
using DuckDB.NET.Data.Mapping;
using System;

// Define your data class
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float Height { get; set; }
    public DateTime BirthDate { get; set; }
}

// Define the mapping
public class PersonMap : DuckDBAppenderMap<Person>
{
    public PersonMap()
    {
        Map(p => p.Id);        // Maps to column 0
        Map(p => p.Name);      // Maps to column 1
        Map(p => p.Height);    // Maps to column 2
        Map(p => p.BirthDate); // Maps to column 3
    }
}

// Use the mapped appender
using var connection = new DuckDBConnection("DataSource=:memory:");
connection.Open();

// Create table
using (var command = connection.CreateCommand())
{
    command.CommandText = "CREATE TABLE person(id INTEGER, name VARCHAR, height REAL, birth_date TIMESTAMP);";
    command.ExecuteNonQuery();
}

// Prepare data
var people = new[]
{
    new Person { Id = 1, Name = "Alice", Height = 1.65f, BirthDate = new DateTime(1990, 1, 15) },
    new Person { Id = 2, Name = "Bob", Height = 1.80f, BirthDate = new DateTime(1985, 5, 20) },
    new Person { Id = 3, Name = "Charlie", Height = 1.75f, BirthDate = new DateTime(1992, 8, 30) }
};

// Insert data using mapped appender
using (var appender = connection.CreateAppender<Person, PersonMap>("person"))
{
    appender.AppendRecords(people);
}

// Verify data was inserted
using (var command = connection.CreateCommand())
{
    command.CommandText = "SELECT COUNT(*) FROM person";
    var count = command.ExecuteScalar();
    Console.WriteLine($"Inserted {count} records");
}

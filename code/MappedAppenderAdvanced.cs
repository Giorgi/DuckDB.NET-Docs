using DuckDB.NET.Data;
using DuckDB.NET.Data.Mapping;
using System;

// Data class with only some properties
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Map with DefaultValue and NullValue
public class EmployeeMap : DuckDBAppenderMap<Employee>
{
    public EmployeeMap()
    {
        Map(e => e.Id);        // Column 0: id (INTEGER)
        Map(e => e.Name);      // Column 1: name (VARCHAR)
        DefaultValue();        // Column 2: department (VARCHAR) - use table default
        NullValue();           // Column 3: manager_id (INTEGER) - insert NULL
    }
}

using var connection = new DuckDBConnection("DataSource=:memory:");
connection.Open();

// Create table with default value
using (var command = connection.CreateCommand())
{
    command.CommandText = @"
        CREATE TABLE employees(
            id INTEGER, 
            name VARCHAR, 
            department VARCHAR DEFAULT 'General',
            manager_id INTEGER
        );";
    command.ExecuteNonQuery();
}

// Insert data
var employees = new[]
{
    new Employee { Id = 1, Name = "Alice" },
    new Employee { Id = 2, Name = "Bob" },
    new Employee { Id = 3, Name = "Charlie" }
};

using (var appender = connection.CreateAppender<Employee, EmployeeMap>("employees"))
{
    appender.AppendRecords(employees);
}

// Query results
using (var command = connection.CreateCommand())
{
    command.CommandText = "SELECT id, name, department, manager_id FROM employees ORDER BY id";
    using var reader = command.ExecuteReader();
    
    Console.WriteLine("Id | Name    | Department | Manager");
    Console.WriteLine("---|---------|------------|--------");
    
    while (reader.Read())
    {
        var id = reader.GetInt32(0);
        var name = reader.GetString(1);
        var department = reader.GetString(2);
        var managerId = reader.IsDBNull(3) ? "NULL" : reader.GetInt32(3).ToString();
        
        Console.WriteLine($"{id,2} | {name,-7} | {department,-10} | {managerId}");
    }
}

// Output:
// Id | Name    | Department | Manager
// ---|---------|------------|--------
//  1 | Alice   | General    | NULL
//  2 | Bob     | General    | NULL
//  3 | Charlie | General    | NULL

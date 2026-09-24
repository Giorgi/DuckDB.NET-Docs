using DuckDB.NET.Data;
using DuckDB.NET.Data.Mapping;

namespace Samples;

// Shared types and helpers referenced by the documentation snippets.
// This file is not included in any documentation page.
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float Height { get; set; }
    public DateTime BirthDate { get; set; }
}

public class PersonMap : DuckDBAppenderMap<Person>
{
    public PersonMap()
    {
        Map(p => p.Id);
        Map(p => p.Name);
        Map(p => p.Height);
        Map(p => p.BirthDate);
    }
}

public class MyData
{
    public int Id { get; set; }
    public float Value { get; set; }
    public DateTime Date { get; set; }
}

public record Employee(int Id, string Name, double Salary);

public class EmployeeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public static class Helpers
{
    public static bool IsPrime(int value)
    {
        for (var i = 2; i <= Math.Sqrt(value); i++)
        {
            if (value % i == 0)
            {
                return false;
            }
        }

        return value >= 2;
    }

    public static IEnumerable<Employee> GetEmployees(int count) =>
        Enumerable.Range(1, count).Select(i => new Employee(i, $"Employee {i}", i * 1000d));

    public static async IAsyncEnumerable<Employee> FetchEmployeesAsync(int count)
    {
        foreach (var employee in GetEmployees(count))
        {
            await Task.Yield();
            yield return employee;
        }
    }

    public static IEnumerable<Employee> FetchEmployees(int count, IEnumerable<string> columns) => GetEmployees(count);

    public static IEnumerable<Employee> FetchEmployees(int count, string? prefix, IReadOnlyList<ProjectedColumn> projected) => GetEmployees(count);

    public static IEnumerable<Employee> FetchAll(IReadOnlyList<ProjectedColumn> projected) => GetEmployees(3);
}

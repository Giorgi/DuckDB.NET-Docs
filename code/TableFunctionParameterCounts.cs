using DuckDB.NET.Data;
using static Samples.Helpers;

namespace Samples.Snippets;

public static class TableFunctionParameterCounts
{
    public static void Run(DuckDBConnection connection, List<Employee> employees)
    {
        #region Example
        // No parameters
        connection.RegisterTableFunction("all_employees",
            () => employees.AsEnumerable(),
            e => new { e.Id, e.Name });

        // Four parameters
        connection.RegisterTableFunction("ext_four",
            (int start, int count, string prefix, double multiplier) =>
                Enumerable.Range(start, count)
                    .Select(i => new Employee(i, $"{prefix}{i}", i * multiplier)),
            e => new { e.Id, e.Name, e.Salary });
        #endregion
    }
}

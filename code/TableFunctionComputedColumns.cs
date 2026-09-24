using DuckDB.NET.Data;
using static Samples.Helpers;

namespace Samples.Snippets;

public static class TableFunctionComputedColumns
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
        // Computed columns
        connection.RegisterTableFunction("ext_computed",
            (int count) => GetEmployees(count),
            e => new { FullName = "Dr. " + e.Name, DoubleSalary = e.Salary * 2 });

        // Object initializer
        connection.RegisterTableFunction("ext_init",
            (int count) => GetEmployees(count),
            e => new EmployeeDto { Id = e.Id, Name = e.Name });
        #endregion
    }
}

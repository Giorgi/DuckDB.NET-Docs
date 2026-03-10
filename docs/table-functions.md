# Table Functions

## High-Level API

> [!TIP]
> Introduced in DuckDB.NET 1.5.0, this is the recommended way to register table functions for most use cases.

Use a projection expression to define both the columns and the mapper in one place:

```cs
connection.RegisterTableFunction("employees",
    (int count) => GetEmployees(count),
    e => new { e.Id, e.Name });
```

The first argument is the function name. The second is a data function that receives the SQL parameters and returns an `IEnumerable<T>`. The third is a projection expression that defines which properties to expose as columns — column names and types are extracted automatically from the expression.

```sql
SELECT * FROM employees(3);
```

The projection supports anonymous types, object initializers, and computed columns:

```cs
// Computed columns
connection.RegisterTableFunction("ext_computed",
    (int count) => GetEmployees(count),
    e => new { FullName = "Dr. " + e.Name, DoubleSalary = e.Salary * 2 });

// Object initializer
connection.RegisterTableFunction("ext_init",
    (int count) => GetEmployees(count),
    e => new EmployeeDto { Id = e.Id, Name = e.Name });
```

The API supports zero to four parameters:

```cs
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
```

### Named Parameters

DuckDB supports named parameters for table functions. Use the `[Named]` attribute to mark parameters as named:

```cs
connection.RegisterTableFunction("employees",
    (int count, [Named] string? prefix) =>
        GetEmployees(count).Select(e => e with { Name = (prefix ?? "") + e.Name }),
    e => new { e.Id, e.Name });
```

In SQL, positional parameters come first, and named parameters use the `=` syntax:

```sql
SELECT * FROM employees(3, prefix = 'Dr. ');

-- Named parameters are optional — omit them and they're NULL
SELECT * FROM employees(3);
```

You can have multiple named parameters:

```cs
connection.RegisterTableFunction("employees",
    (int count, [Named] string? prefix, [Named] double? multiplier) =>
        GetEmployees(count).Select(e => e with
        {
            Name = (prefix ?? "") + e.Name,
            Salary = e.Salary * (multiplier ?? 1)
        }),
    e => new { e.Id, e.Name, e.Salary });
```

By default, the SQL parameter name matches the C# parameter name. You can override it with a custom name:

```cs
connection.RegisterTableFunction("employees",
    (int count, [Named("max_rows")] int? limit) => GetEmployees(limit ?? count),
    e => new { e.Id, e.Name });
```

```sql
SELECT * FROM employees(10, max_rows = 2);
```

Named parameters should typically be nullable types (`string?`, `int?`) since they're optional in SQL. If a named parameter is non-nullable and the caller omits it, DuckDB.NET throws a clear error.

### Async Data Sources

Async data sources work with `ToBlockingEnumerable()`:

```cs
connection.RegisterTableFunction("ext_async",
    (int count) => FetchEmployeesAsync(count).ToBlockingEnumerable(),
    e => new { e.Id, e.Name });
```

## Low-Level API

The low-level API gives you full control over column definitions and the mapper callback. Call one of the [RegisterTableFunction](xref:DuckDB.NET.Data.DuckDBConnection.RegisterTableFunction*) overloads specifying the function name, input parameter type(s), callback for returning result set and a callback function that maps an item from the result set.

```cs
connection.RegisterTableFunction<string, int>("github_search", (parameters) =>
{
    var term = parameters[0].GetValue<string>();
    var stars = parameters[1].GetValue<int>();

    var client = new GitHubClient(new ProductHeaderValue("DuckDB-Table-Valued-Function"));

    var request = new SearchRepositoriesRequest(term)
    {
        Stars = new Octokit.Range(stars, SearchQualifierOperator.GreaterThan)
    };

    var result = client.Search.SearchRepo(request).ConfigureAwait(false).GetAwaiter().GetResult();

    return new TableFunction(new List<ColumnInfo>()
    {
        new ColumnInfo("name", typeof(string)),
        new ColumnInfo("description", typeof(string)),
        new ColumnInfo("stargazers", typeof(int)),
        new ColumnInfo("url", typeof(string)),
        new ColumnInfo("owner", typeof(string)),
    }, result.Items);
}, (item, writers, rowIndex) =>
{
    var repo = (Repository)item;
    writers[0].WriteValue(repo.Name, rowIndex);
    writers[1].WriteValue(repo.Description, rowIndex);
    writers[2].WriteValue(repo.StargazersCount, rowIndex);
    writers[3].WriteValue(repo.Url, rowIndex);
    writers[4].WriteValue(repo.Owner.Login, rowIndex);
});
```

The first callback, `resultCallback`, provides access to the parameters that the function was invoked with and returns a `TableFunction` instance that describes the columns and an `IEnumerable<>` that serves as a result set. The second callback, `mapperCallback`, will be invoked by DuckDB.NET for every item in the result set. In this callback you should use the provided `IDuckDBDataWriter` array to write the item back to DuckDB.

After registering the table function you can use it just like any other table:

```cs
var data = connection.Query<(string, string, int, string, string)>("SELECT * FROM github_search('duckdb', 400);");
```

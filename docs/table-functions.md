---
description: "Write DuckDB table-valued functions in C#: projection expressions, named parameters, projection pushdown, and the low-level API."
---

# Table Functions

## High-Level API

> [!TIP]
> Introduced in DuckDB.NET 1.5.0, this is the recommended way to register table functions for most use cases.

Use a projection expression to define both the columns and the mapper in one place:

[!code-csharp[](../code/TableFunctionBasic.cs)]

The first argument is the function name. The second is a data function that receives the SQL parameters and returns an `IEnumerable<T>`. The third is a projection expression that defines which properties to expose as columns - column names and types are extracted automatically from the expression.

```sql
SELECT * FROM employees(3);
```

The projection supports anonymous types, object initializers, and computed columns:

[!code-csharp[](../code/TableFunctionComputedColumns.cs)]

The API supports zero to four parameters:

[!code-csharp[](../code/TableFunctionParameterCounts.cs)]

### Named Parameters

DuckDB supports named parameters for table functions. Use the [`[Named]`](xref:DuckDB.NET.Data.NamedAttribute) attribute to mark parameters as named:

[!code-csharp[](../code/TableFunctionNamedParameter.cs)]

In SQL, positional parameters come first, and named parameters use the `=` syntax:

```sql
SELECT * FROM employees(3, prefix = 'Dr. ');

-- Named parameters are optional - omit them and they're NULL
SELECT * FROM employees(3);
```

You can have multiple named parameters:

[!code-csharp[](../code/TableFunctionMultipleNamedParameters.cs)]

By default, the SQL parameter name matches the C# parameter name. You can override it with a custom name:

[!code-csharp[](../code/TableFunctionCustomParameterName.cs)]

```sql
SELECT * FROM employees(10, max_rows = 2);
```

Named parameters should typically be nullable types (`string?`, `int?`) since they're optional in SQL. If a named parameter is non-nullable and the caller omits it, DuckDB.NET throws a clear error.

### Projection Pushdown

When DuckDB executes a query like `SELECT name FROM my_func()`, it only needs the `name` column. With projection pushdown, your data function receives the list of requested columns so it can skip fetching unnecessary data - useful when reading from remote APIs, databases, or other expensive sources.

Add `IReadOnlyList<ProjectedColumn>` as the **first** parameter of your data function:

[!code-csharp[](../code/TableFunctionProjectionPushdown.cs)]

```sql
-- projected will contain: [{ Index: 1, Name: "Name", Type: typeof(string) }]
SELECT name FROM employees(3);

-- projected will contain: [{ Index: 0, ... }, { Index: 2, ... }]
SELECT id, salary FROM employees(3);

-- projected will contain all three columns
SELECT * FROM employees(3);
```

Each [`ProjectedColumn`](xref:DuckDB.NET.Data.ProjectedColumn) has three properties:

| Property | Type     | Description                                    |
|----------|----------|------------------------------------------------|
| `Index`  | `int`    | Zero-based index in the original column list   |
| `Name`   | `string` | Column name from the [`ColumnInfo`](xref:DuckDB.NET.Data.ColumnInfo) definition   |
| `Type`   | `Type`   | .NET type from the `ColumnInfo` definition     |

The projection parameter:

- Must be the **first** parameter - placing it elsewhere throws `InvalidOperationException`
- Cannot have the `[Named]` attribute
- Is not exposed as a SQL parameter - it's injected automatically by DuckDB.NET
- Works with zero to three additional SQL parameters (positional and/or named)

[!code-csharp[](../code/TableFunctionProjectionPushdownVariants.cs)]

> [!NOTE]
> Without projection pushdown, data is fetched eagerly at bind time. With projection pushdown, data fetching is deferred to init time - after DuckDB determines which columns are needed.

### Async Data Sources

Async data sources work with `ToBlockingEnumerable()`:

[!code-csharp[](../code/TableFunctionAsync.cs)]

## Low-Level API

The low-level API gives you full control over column definitions and the mapper callback. Call one of the [RegisterTableFunction](xref:DuckDB.NET.Data.DuckDBConnection.RegisterTableFunction*) overloads specifying the function name, input parameter type(s), callback for returning result set and a callback function that maps an item from the result set.

[!code-csharp[](../code/TableFunctionGitHub.cs)]

The first callback, `resultCallback`, provides access to the parameters that the function was invoked with and returns a [`TableFunction`](xref:DuckDB.NET.Data.TableFunction) instance that describes the columns and an `IEnumerable<>` that serves as a result set. The second callback, `mapperCallback`, will be invoked by DuckDB.NET for every item in the result set. In this callback you should use the provided [`IDuckDBDataWriter`](xref:DuckDB.NET.Data.DataChunk.Writer.IDuckDBDataWriter) array to write the item back to DuckDB.

After registering the table function you can use it just like any other table:

```cs
var data = connection.Query<(string, string, int, string, string)>("SELECT * FROM github_search('duckdb', 400);");
```

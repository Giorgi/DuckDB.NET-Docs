---
description: "Write DuckDB table-valued functions in C#: projection expressions, named parameters, projection pushdown, and the low-level API."
---

# Table Functions

## High-Level API

> [!TIP]
> Introduced in DuckDB.NET 1.5.0, this is the recommended way to register table functions for most use cases.

Use a projection expression to define both the columns and the mapper in one place:

[!code-csharp[](../code/TableFunctionBasic.cs#Example)]

The first argument is the function name. The second is a data function that receives the SQL parameters and returns an `IEnumerable<T>`. The third is a projection expression that defines which properties to expose as columns - column names and types are extracted automatically from the expression.

```sql
SELECT * FROM employees(3);
```

The projection supports anonymous types, object initializers, and computed columns:

[!code-csharp[](../code/TableFunctionComputedColumns.cs#Example)]

The API supports zero to four parameters:

[!code-csharp[](../code/TableFunctionParameterCounts.cs#Example)]

> [!TIP]
> When the data function returns an array or a collection such as `List<T>`, DuckDB.NET tells DuckDB how many rows there are, which helps it plan the query. See [Row Count Hint](#row-count-hint).

### Named Parameters

DuckDB supports named parameters for table functions. Use the [`[Named]`](xref:DuckDB.NET.Data.NamedAttribute) attribute to mark parameters as named:

[!code-csharp[](../code/TableFunctionNamedParameter.cs#Example)]

In SQL, positional parameters come first, and named parameters use the `=` syntax:

```sql
SELECT * FROM employees(3, prefix = 'Dr. ');

-- Named parameters are optional - omit them and they're NULL
SELECT * FROM employees(3);
```

You can have multiple named parameters:

[!code-csharp[](../code/TableFunctionMultipleNamedParameters.cs#Example)]

By default, the SQL parameter name matches the C# parameter name. You can override it with a custom name:

[!code-csharp[](../code/TableFunctionCustomParameterName.cs#Example)]

```sql
SELECT * FROM employees(10, max_rows = 2);
```

Named parameters should typically be nullable types (`string?`, `int?`) since they're optional in SQL. If a named parameter is non-nullable and the caller omits it, DuckDB.NET throws a clear error.

### Projection Pushdown

When DuckDB executes a query like `SELECT name FROM my_func()`, it only needs the `name` column. With projection pushdown, your data function receives the list of requested columns so it can skip fetching unnecessary data - useful when reading from remote APIs, databases, or other expensive sources.

Add `IReadOnlyList<ProjectedColumn>` as the **first** parameter of your data function:

[!code-csharp[](../code/TableFunctionProjectionPushdown.cs#Example)]

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

[!code-csharp[](../code/TableFunctionProjectionPushdownVariants.cs#Example)]

> [!NOTE]
> Without projection pushdown, data is fetched eagerly at bind time. With projection pushdown, data fetching is deferred to init time - after DuckDB determines which columns are needed.

### Async Data Sources

Async data sources work with `ToBlockingEnumerable()`:

[!code-csharp[](../code/TableFunctionAsync.cs#Example)]

## Low-Level API

The low-level API gives you full control over column definitions and the mapper callback. Call one of the [RegisterTableFunction](xref:DuckDB.NET.Data.DuckDBConnection.RegisterTableFunction*) overloads specifying the function name, input parameter type(s), callback for returning result set and a callback function that maps an item from the result set.

[!code-csharp[](../code/TableFunctionGitHub.cs#Example)]

The first callback, `resultCallback`, provides access to the parameters that the function was invoked with and returns a [`TableFunction`](xref:DuckDB.NET.Data.TableFunction) instance that describes the columns and an `IEnumerable<>` that serves as a result set. The second callback, `mapperCallback`, will be invoked by DuckDB.NET for every item in the result set. In this callback you should use the provided [`IDuckDBDataWriter`](xref:DuckDB.NET.Data.DataChunk.Writer.IDuckDBDataWriter) array to write the item back to DuckDB.

Read each parameter with `GetValue<T>` as the .NET type of its DuckDB type, for example `int` for an INTEGER or `string` for a VARCHAR. `object`, an interface of that type and its nullable form work too. A HUGEINT or UHUGEINT is an `Int128` or `UInt128`, and can also be read as `BigInteger`. A DATE can also be read as `DuckDBDateOnly`, a TIME or TIME_NS as `DuckDBTimeOnly` and a timestamp as `DateTimeOffset` at UTC, each also in its nullable form. Any other type throws `InvalidCastException`, so an INTEGER parameter can't be read as `long` the way a column can. A BLOB, LIST, ARRAY, STRUCT, MAP, UNION, ENUM, VARINT or BIT parameter can't be read and throws `NotImplementedException`. An `object` parameter can receive any of them. Check `IsNull()` before you read a parameter, because `GetValue` doesn't handle a NULL argument.

The mapper writes values the way the [Standard Appender](standard-appender.md) does. A number must have the exact type of its column, such as `int` for an INTEGER column, or it stores a wrong value. Other values are checked against the column, and a value the column can't hold throws `InvalidOperationException`.

After registering the table function you can use it just like any other table:

```csharp
var data = connection.Query<(string, string, int, string, string)>("SELECT * FROM github_search('duckdb', 400);");
```

> [!NOTE]
> `connection.Query` comes from [Dapper](https://github.com/DapperLib/Dapper), used here for brevity. Dapper is not required to register or use table functions.

### Row Count Hint

DuckDB plans a query better when it knows how many rows a table function returns. DuckDB.NET tells it the count by itself when the data is an array or a collection. The length of an array is passed as exact, and the count of a `List<T>` or another collection as an estimate. The high-level API also passes the count of a LINQ result such as `list.Select(...)` when it is known without reading the data. The low-level API sees the data only as an `IEnumerable` and counts a collection through `ICollection` or `IReadOnlyCollection<object>`, so a `HashSet<int>` gets no count there. An iterator, such as a method with `yield return`, has no count until it is read, so nothing is passed for it. With [projection pushdown](#projection-pushdown) the data is created after the query is planned, so nothing is passed either.

In the low-level API you can pass a number yourself, for example for an iterator whose length you know. To do so, give a [`CardinalityHint`](xref:DuckDB.NET.Data.CardinalityHint) as the third argument of the `TableFunction` constructor, like this:

```csharp
return new TableFunction(columns, items, new CardinalityHint(1000));
```

By default the number is an estimate, which DuckDB uses only for planning. Pass `IsExact: true` only when the function returns exactly that many rows. DuckDB then also takes the number as the most rows the function can return. It can drop a `LIMIT` larger than that, or add integers without checking for overflow, so a function that returns more rows than it said can give wrong results.

# DuckDB.NET Basic Usage

## SQL Execution

To execute SQL statements in DuckDB.NET you need to create a `DuckDBConnection` object that acts as an entry point for executing SQL statements. After you create a connection, create a command associated with it and use one of the following methods to execute the command:

- `ExecuteNonQuery` - Execute SQL that does not return results such as DDL or changes the data by executing `UPDATE`, `INSERT`, or `DELETE` statements
- `ExecuteScalar` - Execute SQL that returns a single, scalar value.
- `ExecuteReader` - Executes SQL and returns `DuckDBDataReader` that can be used to consume the result set.

[!code-csharp[](../code/ExecuteNonQuery.cs "ExecuteNonQuery Example")]
[!code-csharp[](../code/ExecuteScalar.cs "ExecuteScalar Example")]
[!code-csharp[](../code/ExecuteReader.cs "ExecuteReader Example")]

## Parameterized statements

When building SQL commands, always use parameterized query instead of string concatenation. This helps to protect against [SQL injection](http://xkcd.com/327/), improves type safety, and can result in improved performance.

[DuckDB supports three syntaxes](https://duckdb.org/docs/sql/query_syntax/prepared_statements) for denoting parameters in prepared statements: auto-incremented (`?`), positional (`$1`), and named (`$param`). All three syntaxes are supported in DuckDB.NET:

```cs
using var connection = new DuckDBConnection("DataSource=:memory:");
connection.Open();

using var command = connection.CreateCommand();
command.CommandText = "SELECT * FROM person WHERE starts_with(name, $name_start_letter) AND age >= $minimum_age;";

command.Parameters.Add(new DuckDBParameter("minimum_age", 40));
command.Parameters.Add(new DuckDBParameter("name_start_letter", "B"));

using var reader = command.ExecuteReader();
```

## Batching

You can execute multiple statements in a single go by concatenating the statements and delimiting them with a semicolon:

```cs
using var connection = new DuckDBConnection("DataSource=:memory:");
connection.Open();

using var command = connection.CreateCommand();
command.CommandText = "INSTALL httpfs;LOAD httpfs;";
command.ExecuteNonQuery();
```

If the statements return data, you can consume the multiple result sets using the `NextResult` method:

```cs
using var connection = new DuckDBConnection("DataSource=:memory:");
connection.Open();

using var command = connection.CreateCommand();
command.CommandText = "SELECT * FROM weather ORDER BY city, temp_lo;SELECT DISTINCT city FROM weather;";
using var reader = command.ExecuteNonQuery();

do
{
  //consume data from the reader
} while (reader.NextResult());
```

## Materialized and Streaming mode

When executing a Select statement with a `DuckDBCommand`, DuckDB.NET will by default use materialized execution mode. In this mode, DuckDB will fetch the whole
result set in memory which might result in high memory usage. An alternative is to use streaming execution mode, in which case DuckDB will fetch the result
set chunk by chunk and use a lower amount of memory but this may result in slower query execution. To use streaming mode set the [`UseStreamingMode`](xref:DuckDB.NET.Data.DuckDBCommand.UseStreamingMode) property to true.
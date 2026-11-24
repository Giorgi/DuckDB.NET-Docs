---
description: "Execute SQL with DuckDB.NET: ExecuteNonQuery, ExecuteScalar, and ExecuteReader, plus parameterized statements, prepared commands, batching, and streaming mode."
---

# DuckDB.NET Basic Usage

## SQL Execution

To execute SQL statements in DuckDB.NET you need to create a [`DuckDBConnection`](xref:DuckDB.NET.Data.DuckDBConnection) object that acts as an entry point for executing SQL statements. After you create a connection, create a command associated with it and use one of the following methods to execute the command:

- `ExecuteNonQuery` - Execute SQL that does not return results, such as DDL, or that changes data. It returns the number of rows affected by an `INSERT`, `UPDATE`, `DELETE`, `MERGE` or `COPY` statement, and -1 for any other statement, such as `CREATE TABLE` or `SET`.
- `ExecuteScalar` - Execute SQL that returns a single, scalar value.
- `ExecuteReader` - Executes SQL and returns [`DuckDBDataReader`](xref:DuckDB.NET.Data.DuckDBDataReader) that can be used to consume the result set.

When a command holds several statements, `ExecuteNonQuery` adds up the rows of the statements that report a count, and returns -1 if none of them does. [`RecordsAffected`](xref:DuckDB.NET.Data.DuckDBDataReader.RecordsAffected) on a data reader follows the same rule.

[!code-csharp[](../code/ExecuteNonQuery.cs#Example "ExecuteNonQuery Example")]
[!code-csharp[](../code/ExecuteScalar.cs#Example "ExecuteScalar Example")]
[!code-csharp[](../code/ExecuteReader.cs#Example "ExecuteReader Example")]

## Parameterized statements

When building SQL commands, always use parameterized query instead of string concatenation. This helps to protect against [SQL injection](http://xkcd.com/327/), improves type safety, and can result in improved performance.

[DuckDB supports three syntaxes](https://duckdb.org/docs/sql/query_syntax/prepared_statements) for denoting parameters in prepared statements: auto-incremented (`?`), positional (`$1`), and named (`$param`). All three syntaxes are supported in DuckDB.NET:

[!code-csharp[](../code/BasicUsageParameterizedStatement.cs#Example)]

### Parameter values

A parameter is sent as the type of its value. DuckDB often knows the type of the parameter, for example from the column it is compared with. If that is an integer type, an integer value is converted to it when it fits. A `decimal` gets the width and scale of a DECIMAL parameter when it fits them. Any other value is sent as its own type, and DuckDB casts it as it would cast a SQL literal. So for an INTEGER parameter a `decimal` 2.5 becomes 3 and a `double` 2.5 becomes 2, because DuckDB rounds a DECIMAL half away from zero and a DOUBLE half to even. A value too large for its parameter is refused by DuckDB.

An `Int128` or `UInt128` is sent as a HUGEINT or UHUGEINT. An enum is sent as its number for an integer parameter and as its name otherwise. A value with no DuckDB type of its own is sent as text, formatted with the invariant culture, so the culture of the machine never changes what DuckDB gets.

If you set the `DbType` of a parameter, the value is converted to the .NET type of that `DbType` when the command runs. A value that doesn't fit that type, or has a fraction for an integer `DbType`, is refused with an `InvalidCastException`. So `DbType.Int64` takes an `int`, and `DbType.Date` takes a `DateTime` and keeps its date. `DbType.Single` and `DbType.Double` take any number, and one too large becomes infinity. A `double` or `float` with `DbType.Decimal` keeps 15 or 7 significant digits, as a C# cast does. `DbType.Int32` with 2.5, `DbType.Int32` with `"42"` and `DbType.String` with 42 are refused, because a `DbType` never parses text or turns a number into text. Call `ResetDbType()` to remove a `DbType` you set.

## Prepared commands

If you execute the same command many times, call [`Prepare`](xref:DuckDB.NET.Data.DuckDBCommand.Prepare) before the first execution. It prepares the statement and keeps it on the connection until the connection closes or the same text is prepared again. Every prepared command on that connection with the same text uses the newest statement. Later executions skip parsing and preparing the statement, so set new parameter values and execute the command again, like this:

[!code-csharp[](../code/BasicUsagePrepare.cs#Example)]

`Prepare` needs an open connection and throws `InvalidOperationException` without one. An error in the SQL is reported by `Prepare` instead of by the first execution. If the connection is closed and opened again, the next execution prepares the statement again. Changing `CommandText` or `Connection` undoes the preparation.

A prepared statement keeps the parameter types it had when it was prepared. If a schema change changes the type of a parameter, for example when you alter a column, call `Prepare` again. A command that holds several statements is not prepared, and each of its statements is still prepared just before it runs.

## Batching

You can execute multiple statements in a single go by concatenating the statements and delimiting them with a semicolon:

[!code-csharp[](../code/BasicUsageBatching.cs#Example)]

If the statements return data, you can consume the multiple result sets using the `NextResult` method:

[!code-csharp[](../code/BasicUsageMultipleResultSets.cs#Example)]

## Materialized and Streaming mode

When executing a Select statement with a [`DuckDBCommand`](xref:DuckDB.NET.Data.DuckDBCommand), DuckDB.NET will by default use materialized execution mode. In this mode, DuckDB will fetch the whole
result set in memory which might result in high memory usage. An alternative is to use streaming execution mode, in which case DuckDB will fetch the result
set chunk by chunk and use a lower amount of memory but this may result in slower query execution. To use streaming mode set the [`UseStreamingMode`](xref:DuckDB.NET.Data.DuckDBCommand.UseStreamingMode) property to true.
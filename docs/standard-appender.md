# Standard Appender

The standard appender is a lower-level API that uses [CreateAppender](xref:DuckDB.NET.Data.DuckDBConnection.CreateAppender(System.String)) to efficiently add rows to the database. It requires manual row creation with `CreateRow` and `AppendValue` methods.

Use this approach for maximum performance when type safety is not needed, or when you need fine-grained control over the insertion process.

> [!CAUTION]
> When using the standard appender, data types **MUST** match the length of the database types **exactly**. For example when inserting into a UBIGINTEGER column, a ulong such as `0UL` must be used. Writing just `0` will cause data corruption by writing adjacent memory to the database.

## Example
[!code-csharp[](../code/ManagedAppender.cs)]

## Data Importing from Files

For importing data from CSV, Parquet, JSON and other file types see the DuckDB documentation for [Data Importing](https://duckdb.org/docs/data/overview).

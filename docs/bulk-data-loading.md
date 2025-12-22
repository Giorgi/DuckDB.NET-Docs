# Bulk Data Loading

In DuckDB [Appender](https://duckdb.org/docs/data/appender) can be used to efficiently add rows to the database. Appender always appends to a single table in the database.

There are two ways to use appender in .NET:

1. **[Mapped Appender](mapped-appender.md) (Recommended)** - A strongly-typed appender that provides:
   - **Type Safety**: Compile-time type checking for property mappings
   - **Automatic Validation**: Runtime verification that mapped types match the table schema
   - **Declarative Mapping**: Clear property-to-column definitions that prevent ordering errors
   - **Better Maintainability**: Mapping logic separated from data insertion code

2. **Standard Appender** - A lower-level API using [CreateAppender](xref:DuckDB.NET.Data.DuckDBConnection.CreateAppender(System.String)) that requires manual row creation with `CreateRow` and `AppendValue` methods. Use this for maximum performance when type safety is not needed.

For most use cases, the [Mapped Appender](mapped-appender.md) is recommended as it eliminates common errors while maintaining excellent performance. Make sure to `Dispose` the appender to avoid data loss.

> [!CAUTION]
> When using the standard appender, data types **MUST** match the length of the database types **exactly**. For example when inserting into a UBIGINTEGER column, a ulong such as `0UL` must be used. Writing just `0` will cause data corruption by writing adjacent memory to the database.

## Example
[!code-csharp[](../code/ManagedAppender.cs)]

For importing data from CSV, Parquet, JSON and other file types see the DuckDB documentation for [Data Importing](https://duckdb.org/docs/data/overview).

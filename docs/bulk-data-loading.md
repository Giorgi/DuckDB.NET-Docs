# Bulk Data Loading

In DuckDB [Appender](https://duckdb.org/docs/data/appender) can be used to efficiently add rows to the database. Appender always appends to a single table in the database.

To use appender in .NET, call [CreateAppender](xref:DuckDB.NET.Data.DuckDBConnection.CreateAppender(System.String)) method to create and initialize an appender. After you create an appender, use `CreateRow` and `AppendValue` methods to create rows and append data. Make sure to Dispose the appender to avoid data loss.

[!code-csharp[](../code/ManagedAppender.cs)]

For importing data from CSV, Parquet, JSON and other file types see the DuckDB documentation for [Data Importing](https://duckdb.org/docs/data/overview).

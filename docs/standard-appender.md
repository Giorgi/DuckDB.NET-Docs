# Standard Appender

The standard appender is a lower-level API that uses [CreateAppender](xref:DuckDB.NET.Data.DuckDBConnection.CreateAppender(System.String)) to efficiently add rows to the database. Rows are added either with [`CreateRow`](xref:DuckDB.NET.Data.DuckDBAppender.CreateRow) and [`AppendValue`](xref:DuckDB.NET.Data.IDuckDBAppenderRow.AppendValue*), or with the scoped `AppendRow` callback described [below](#appending-rows-with-a-callback).

Use this approach for maximum performance, or when you need fine-grained control over the insertion process. The appended values must still match the table's column order and types exactly - unlike with the [Mapped Appender](mapped-appender.md), mismatches are not caught up front and surface only at runtime.

> [!CAUTION]
> When using the standard appender, data types **MUST** match the length of the database types **exactly**. For example when inserting into a UBIGINTEGER column, a ulong such as `0UL` must be used. Writing just `0` will cause data corruption by writing adjacent memory to the database.

## Example
[!code-csharp[](../code/ManagedAppender.cs)]

> [!TIP]
> `CreateRow` allocates a new row object on every call. Prefer [AppendRow](#appending-rows-with-a-callback) - it reuses a single row instance and avoids that per-row allocation, so it is the recommended approach for bulk loading. Reach for `CreateRow` only when you need an independent row instance whose lifetime you control.

## Appending Rows with a Callback

[`AppendRow`](xref:DuckDB.NET.Data.DuckDBAppender.AppendRow*) is the recommended, lower-allocation way to add rows with the standard appender. It scopes the row to a callback and calls `EndRow` for you, and - unlike `CreateRow`, which allocates a new row object per call - it reuses a single row instance across calls. That avoids the per-row allocation, a meaningful saving when loading large numbers of rows.

[!code-csharp[](../code/StandardAppenderAppendRow.cs)]

The row passed to the callback is valid only for the duration of that callback - do not store it or use it afterwards - and the callback must not call other methods on the same appender.

> [!TIP]
> For large or hot-path loads, prefer the overload that takes a `state` argument together with a `static` callback, as shown above. A callback that captures variables allocates a closure on every row; passing the captured data through `state` keeps the append allocation-free. For occasional use where allocation is not a concern, the single-argument overload is more concise:
>
> ```csharp
> appender.AppendRow(row => row.AppendValue(1).AppendValue(3));
> ```

## Behavior When a Row Fails

If the callback throws, or the row is left incomplete, `AppendRow` discards the failing row and the appender is *faulted*: no further rows can be appended. The rows completed before the failure are still written when you `Close` (or `Dispose`) the appender.

[!code-csharp[](../code/StandardAppenderFailedRow.cs)]

DuckDB flushes completed rows to storage in batches, so the exact number of rows already persisted when a failure occurs is not defined. If you need all-or-nothing semantics, wrap the append in a transaction and roll it back on failure:

[!code-csharp[](../code/StandardAppenderTransaction.cs)]

## Discarding In-Progress Rows

Call [`Clear()`](xref:DuckDB.NET.Data.DuckDBAppender.Clear) to discard any buffered rows without closing the appender. This lets you roll back a batch and continue appending:

[!code-csharp[](../code/StandardAppenderClear.cs)]

## Data Importing from Files

For importing data from CSV, Parquet, JSON and other file types see the DuckDB documentation for [Data Importing](https://duckdb.org/docs/data/overview).

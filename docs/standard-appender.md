# Standard Appender

The standard appender is a lower-level API that uses [CreateAppender](xref:DuckDB.NET.Data.DuckDBConnection.CreateAppender(System.String)) to efficiently add rows to the database. Rows are added either with `CreateRow` and `AppendValue`, or with the scoped `AppendRow` callback described [below](#appending-rows-with-a-callback).

Use this approach for maximum performance when type safety is not needed, or when you need fine-grained control over the insertion process.

> [!CAUTION]
> When using the standard appender, data types **MUST** match the length of the database types **exactly**. For example when inserting into a UBIGINTEGER column, a ulong such as `0UL` must be used. Writing just `0` will cause data corruption by writing adjacent memory to the database.

## Example
[!code-csharp[](../code/ManagedAppender.cs)]

> [!TIP]
> `CreateRow` allocates a new row object on every call. Prefer [AppendRow](#appending-rows-with-a-callback) — it reuses a single row instance and avoids that per-row allocation, so it is the recommended approach for bulk loading. Reach for `CreateRow` only when you need an independent row instance whose lifetime you control.

## Appending Rows with a Callback

`AppendRow` is the recommended, lower-allocation way to add rows with the standard appender. It scopes the row to a callback and calls `EndRow` for you, and — unlike `CreateRow`, which allocates a new row object per call — it reuses a single row instance across calls. That avoids the per-row allocation, a meaningful saving when loading large numbers of rows.

```csharp
using (var appender = connection.CreateAppender("AppenderTest"))
{
    for (var i = 0; i < rows; i++)
    {
        appender.AppendRow(i, static (row, value) =>
        {
            row.AppendValue(value).AppendValue(value + 2);
        });
    }
}
```

The row passed to the callback is valid only for the duration of that callback — do not store it or use it afterwards — and the callback must not call other methods on the same appender.

> [!TIP]
> For large or hot-path loads, prefer the overload that takes a `state` argument together with a `static` callback, as shown above. A callback that captures variables allocates a closure on every row; passing the captured data through `state` keeps the append allocation-free. For occasional use where allocation is not a concern, the single-argument overload is more concise:
>
> ```csharp
> appender.AppendRow(row => row.AppendValue(1).AppendValue(3));
> ```

## Behavior When a Row Fails

If the callback throws, or the row is left incomplete, `AppendRow` discards the failing row and the appender is *faulted*: no further rows can be appended. The rows completed before the failure are still written when you `Close` (or `Dispose`) the appender.

```csharp
using (var appender = connection.CreateAppender("AppenderTest"))
{
    appender.AppendRow(row => row.AppendValue(1).AppendValue(2)); // completed

    try
    {
        appender.AppendRow(row =>
        {
            row.AppendValue(3).AppendValue(4);
            throw new InvalidOperationException("something went wrong");
        });
    }
    catch (InvalidOperationException)
    {
        // The (3, 4) row is discarded and no more rows may be appended.
    }
} // Dispose -> Close writes the completed (1, 2) row
```

DuckDB flushes completed rows to storage in batches, so the exact number of rows already persisted when a failure occurs is not defined. If you need all-or-nothing semantics, wrap the append in a transaction and roll it back on failure:

```csharp
using var transaction = connection.BeginTransaction();
try
{
    using (var appender = connection.CreateAppender("AppenderTest"))
    {
        for (var i = 0; i < rows; i++)
        {
            appender.AppendRow(i, static (row, value) => row.AppendValue(value).AppendValue(value + 2));
        }
    }

    transaction.Commit();
}
catch
{
    transaction.Rollback(); // Nothing is persisted, including rows already flushed
    throw;
}
```

## Discarding In-Progress Rows

Call `Clear()` to discard any buffered rows without closing the appender. This lets you roll back a batch and continue appending:

```csharp
appender.CreateRow().AppendValue(1).AppendValue(10).EndRow();
appender.Clear(); // Discards the row above
appender.CreateRow().AppendValue(2).AppendValue(20).EndRow();
appender.Close(); // Only (2, 20) is inserted
```

## Data Importing from Files

For importing data from CSV, Parquet, JSON and other file types see the DuckDB documentation for [Data Importing](https://duckdb.org/docs/data/overview).

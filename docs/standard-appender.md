---
description: "Bulk load rows into DuckDB with the low-level standard appender: CreateRow, AppendRow callbacks, failure handling, and transactions."
---

# Standard Appender

The standard appender is a lower-level API that uses [CreateAppender](xref:DuckDB.NET.Data.DuckDBConnection.CreateAppender(System.String)) to efficiently add rows to the database. Rows are added either with [`CreateRow`](xref:DuckDB.NET.Data.DuckDBAppender.CreateRow) and [`AppendValue`](xref:DuckDB.NET.Data.IDuckDBAppenderRow.AppendValue*), or with the scoped `AppendRow` callback described [below](#appending-rows-with-a-callback).

Use this approach for maximum performance, or when you need fine-grained control over the insertion process. The appended values must still match the table's column order and types exactly - unlike with the [Mapped Appender](mapped-appender.md), mismatches are not caught up front and surface only at runtime.

> [!CAUTION]
> The standard appender writes a number as the bytes of its .NET type, without checking it against the column, so every number must have the exact type of its column. For example, a UBIGINT column needs a `ulong` such as `0UL`. A number of any other type stores wrong values.
>
> This includes integer constants. In C# 12, the default for net8.0 projects, `row.AppendValue(1)` fails with error CS0121 because the constant matches several overloads. Later C# versions compile it, but pass the constant as an `sbyte`, which stores a wrong value in an INTEGER column. Write `(int?)1` instead, or pass a variable of the column's type.

## Example
[!code-csharp[](../code/ManagedAppender.cs#Example)]

> [!TIP]
> `CreateRow` allocates a new row object on every call. Prefer [AppendRow](#appending-rows-with-a-callback) - it reuses a single row instance and avoids that per-row allocation, so it is the recommended approach for bulk loading. Reach for `CreateRow` only when you need an independent row instance whose lifetime you control.

## Values Other Than Numbers

Values that are not numbers are checked against the column. A value the column can't hold throws `InvalidOperationException`, for example a `DateTime` appended to a VARCHAR column, and the row stays open for a correct value (see [With CreateRow](#with-createrow)). A string is stored in full, including any NUL characters.

A HUGEINT column takes an `Int128` and a UHUGEINT column a `UInt128`. Both also take a `BigInteger` within the column's range. A `BigInteger` outside that range throws `ArgumentOutOfRangeException`, and one appended to any other column throws `InvalidOperationException`.

A DATE column takes a `DateOnly`, `DuckDBDateOnly` or `DateTime`. A TIME column takes a `TimeOnly`, `DuckDBTimeOnly` or `DateTime`, and stores the time of day of a `DateTime`. A `TimeOnly` appended to a DATE, TIMETZ or timestamp column, or a `DateOnly` appended to a TIME column, throws `InvalidOperationException`.

## Appending Rows with a Callback

[`AppendRow`](xref:DuckDB.NET.Data.DuckDBAppender.AppendRow*) is the recommended, lower-allocation way to add rows with the standard appender. It scopes the row to a callback and calls `EndRow` for you, and - unlike `CreateRow`, which allocates a new row object per call - it reuses a single row instance across calls. That avoids the per-row allocation, a meaningful saving when loading large numbers of rows.

[!code-csharp[](../code/StandardAppenderAppendRow.cs#Example)]

The row passed to the callback is valid only for the duration of that callback - do not store it or use it afterwards - and the callback must not call other methods on the same appender.

> [!TIP]
> For large or hot-path loads, prefer the overload that takes a `state` argument together with a `static` callback, as shown above. A callback that captures variables allocates a closure on every row; passing the captured data through `state` keeps the append allocation-free. For occasional use where allocation is not a concern, the single-argument overload is more concise:
>
> ```csharp
> appender.AppendRow(row => row.AppendValue((int?)1).AppendValue((int?)3));
> ```

## Behavior When a Row Fails

### With AppendRow

If the callback throws, or the row is left incomplete, `AppendRow` discards the failing row and the appender is *faulted*: no further rows can be appended. The rows completed before the failure are still written when you `Close` (or `Dispose`) the appender.

[!code-csharp[](../code/StandardAppenderFailedRow.cs#Example)]

### With CreateRow

A row created with `CreateRow` is written once every column has a value. If appending a value throws, for example because its type doesn't match the column, nothing is written for that column, so you can catch the exception and append a correct value to finish the row:

[!code-csharp[](../code/StandardAppenderCreateRowFailedRow.cs#Retry)]

If you move on without finishing the row, it is discarded: the next `CreateRow` or `AppendRow` call throws and the appender is faulted, just as with `AppendRow`. The rows completed before it are still written when you `Close` (or `Dispose`) the appender.

[!code-csharp[](../code/StandardAppenderCreateRowFailedRow.cs#Skip)]

A row that is still incomplete when the appender is closed, disposed or cleared is discarded without an error.

### Persisted Rows and Transactions

DuckDB flushes completed rows to storage in batches, so the exact number of rows already persisted when a failure occurs is not defined. If you need all-or-nothing semantics, wrap the append in a transaction and roll it back on failure:

[!code-csharp[](../code/StandardAppenderTransaction.cs#Example)]

## Discarding In-Progress Rows

Call [`Clear()`](xref:DuckDB.NET.Data.DuckDBAppender.Clear) to discard any buffered rows without closing the appender. This lets you roll back a batch and continue appending:

[!code-csharp[](../code/StandardAppenderClear.cs#Example)]

## Data Importing from Files

For importing data from CSV, Parquet, JSON and other file types see the DuckDB documentation for [Data Importing](https://duckdb.org/docs/data/overview).

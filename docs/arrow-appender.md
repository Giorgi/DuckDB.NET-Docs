---
description: "Append Apache Arrow record batches and streams to a DuckDB table with DuckDBArrowAppender."
---

# Arrow Appender

The Arrow appender adds [Apache Arrow](https://arrow.apache.org/) record batches to a table. Create it with [`CreateArrowAppender`](xref:DuckDB.NET.Data.DuckDBConnection.CreateArrowAppender(System.String)) and pass each batch to [`AppendBatch`](xref:DuckDB.NET.Data.DuckDBArrowAppender.AppendBatch(Apache.Arrow.RecordBatch)). DuckDB takes the batch through its Arrow C Data Interface, so the values are not marshaled one by one. Use this appender when your data is already in Arrow format, for example when it comes from another Arrow-aware library.

## Example

[!code-csharp[](../code/ArrowAppender.cs#Example)]

The appended data is copied, so you can dispose a batch as soon as `AppendBatch` returns. An appender is not thread-safe.

## Columns and Types

Columns are matched by position, and the field names of the batch are ignored. A batch must have as many columns as the table, or `AppendBatch` throws `ArgumentException`. DuckDB casts each Arrow column to the type of its table column, and a value that can't be cast is an error. So unlike the [Standard Appender](standard-appender.md), a column of the wrong type can't store wrong values.

## Appending Streams

[`AppendStream`](xref:DuckDB.NET.Data.DuckDBArrowAppender.AppendStream(Apache.Arrow.Ipc.IArrowArrayStream)) reads an `IArrowArrayStream` to the end and appends every batch, and [`AppendStreamAsync`](xref:DuckDB.NET.Data.DuckDBArrowAppender.AppendStreamAsync(Apache.Arrow.Ipc.IArrowArrayStream,System.Threading.CancellationToken)) does the same asynchronously. Both return the number of rows appended. Each batch read from the stream is disposed after it is appended, but the stream itself is not, so dispose it yourself.

To copy a query result into a table, pass the stream returned by [`ExecuteArrowStream`](arrow.md). Run the query on a different connection from the appender's, because a connection with an open streaming result can't do the appender's work.

## When an Append Fails

A batch is appended in one piece. If DuckDB can't convert or cast it, none of its rows are appended and the appender is faulted, so no more batches can be appended. The batches appended before the failure are still written when the appender is closed or disposed.

Constraints such as NOT NULL, PRIMARY KEY and UNIQUE are checked later, when the appender flushes. It flushes by itself every 204,800 rows, and when it is closed or disposed. A constraint failure is thrown by the call that flushed, which can be a later `AppendBatch`, `Close` or `Dispose`. The failure drops every batch appended since the last flush, not only the one with the bad row. If you need all-or-nothing, wrap the appends in a transaction, as shown for the [Standard Appender](standard-appender.md#persisted-rows-and-transactions).

[`Clear()`](xref:DuckDB.NET.Data.DuckDBArrowAppender.Clear) drops the data appended since the appender last flushed.

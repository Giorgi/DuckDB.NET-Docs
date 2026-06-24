# Apache Arrow

[Apache Arrow](https://arrow.apache.org/) is a columnar in-memory format for analytical data. DuckDB.NET can hand you a query result directly as Arrow record batches, so you can move data into Arrow-aware libraries without reading it row by row.

DuckDB.NET builds the batches with DuckDB's Arrow C Data Interface. Each DuckDB data chunk becomes an Arrow `RecordBatch` with no per-value marshaling. The Arrow types come from the [`Apache.Arrow`](https://www.nuget.org/packages/Apache.Arrow) package, which ships as a dependency of `DuckDB.NET.Data`, so you do not need to add it yourself.

## Stream Arrow record batches

Call `ExecuteArrowStream` to get an `IArrowArrayStream`. The stream exposes the result `Schema` and reads one `RecordBatch` per DuckDB data chunk. You own the returned stream and must dispose it.

[!code-csharp[](../code/ArrowResultStream.cs "ExecuteArrowStream Example")]

Each column in a batch is an Arrow array. Cast it to the matching Arrow type, such as `StringArray` or `Int32Array`, to read values.

## Stream batches asynchronously

`ExecuteArrowBatchesAsync` returns an `IAsyncEnumerable<RecordBatch>` that you consume with `await foreach`. It produces the same batches as `ExecuteArrowStream` and accepts a `CancellationToken`.

[!code-csharp[](../code/ArrowResultBatchesAsync.cs "ExecuteArrowBatchesAsync Example")]

## Materialized and streaming mode

The Arrow methods honor the same execution modes as the rest of DuckDB.NET. By default DuckDB materializes the whole result in memory before producing batches. Set the [`UseStreamingMode`](xref:DuckDB.NET.Data.DuckDBCommand.UseStreamingMode) property to `true` to fetch the result chunk by chunk with bounded memory. See [Basic Usage](basic-usage.md) for more on materialized and streaming mode.

## Multiple statements

The Arrow methods return only the first result set in the command. They execute statements up to and including that first result set, then stop. Any statement after it does not run, so its side effects (for example a `CREATE TABLE` or `INSERT` after a `SELECT`) are skipped.

If your command has more than one result set, use `ExecuteReader` and walk the results with `NextResult` instead. See [Batching](basic-usage.md#batching). For Arrow, prefer a single-statement command.

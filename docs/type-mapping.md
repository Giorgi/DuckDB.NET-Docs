# DuckDB.NET Type Mappings

The following table lists the mappings between DuckDB types and .NET types. The default type is returned by the [`GetValue`](xref:DuckDB.NET.Data.DuckDBDataReader.GetValue(System.Int32)) method. Non-default types can be read by calling the [`GetFieldValue<T>`](xref:DuckDB.NET.Data.DuckDBDataReader.GetFieldValue``1(System.Int32)) method.

| DuckDB type  | Default .NET type | Default .NET type
| -----------  | ------------      | -----------
| Boolean      | bool              | &nbsp;
| TINYINT      | sbyte             | short, int, long, byte, ushort, uint, ulong
| SMALLINT     | short             | sbyte, int, long, byte, ushort, uint, ulong
| INTEGER      | int               | sbyte, short, long, byte, ushort, uint, ulong
| BIGINT       | long              | sbyte, short, int, byte, ushort, uint, ulong
| HUGEINT      | BigInteger        | sbyte, short, int, long, uint, ulong
| UTINYINT     | byte              | sbyte, short, int, long, ushort, uint, ulong
| USMALLINT    | ushort            | sbyte, short, int, long, byte, uint, ulong
| UINTEGER     | uint              | sbyte, short, int, long, byte, ushort, ulong
| UBIGINT      | ulong             | sbyte, short, int, long, byte, ushort, uint
| UHUGEINT     | BigInteger        | sbyte, short, int, long, uint, ulong
| REAL         | float             | &nbsp;
| DOUBLE       | double            | &nbsp;
| DECIMAL      | decimal           | &nbsp;
| UUID         | Guid              | &nbsp;
| VARCHAR      | string            | &nbsp;
| BLOB         | Stream            | &nbsp;
| BIT          | string            | BitArray
| DATE         | DateTime/DateOnly | DateTime, DateOnly, DuckDBDateOnly
| TIME         | TimeSpan/TimeOnly | DateTime, TimeOnly, DuckDBTimeOnly
| TIMETZ       | DateTimeOffset    | DuckDBTimeTz
| TIMESTAMP    | DateTime          | DuckDBTimestamp
| TIMESTAMP_NS | DateTime          | DuckDBTimestamp
| TIMESTAMP_MS | DateTime          | DuckDBTimestamp
| TIMESTAMP_S  | DateTime          | DuckDBTimestamp
| TIMESTAMPTZ  | DateTime          | DuckDBTimestamp
| INTERVAL     | TimeSpan          | DuckDBInterval

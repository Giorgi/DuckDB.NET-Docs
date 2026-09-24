---
description: "Mappings between DuckDB column types and .NET types when reading data with GetValue and GetFieldValue<T>."
---

# DuckDB.NET Type Mappings

The following table lists the mappings between DuckDB types and .NET types. The default type is returned by the [`GetValue`](xref:DuckDB.NET.Data.DuckDBDataReader.GetValue(System.Int32)) method. Non-default types can be read by calling the [`GetFieldValue<T>`](xref:DuckDB.NET.Data.DuckDBDataReader.GetFieldValue``1(System.Int32)) method. Calling `GetFieldValue<object>` returns the same value as `GetValue`.

| DuckDB type  | Default .NET type | Non-default .NET types
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
| DECIMAL      | decimal           | BigInteger, DuckDBDecimal
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

> [!NOTE]
> A `DECIMAL` value with more significant digits than `System.Decimal` supports (28-29) can't be read as `decimal` — reading it throws `OverflowException`. Such values can still be read losslessly: `GetFieldValue<BigInteger>()` returns the exact value when its fractional part is zero (and throws `InvalidCastException` otherwise), and `GetFieldValue<DuckDBDecimal>()` returns the unscaled value together with the column's width and scale for any `DECIMAL`. `DuckDBDecimal` is also the provider-specific type for `DECIMAL` columns, returned by `GetProviderSpecificValue`.

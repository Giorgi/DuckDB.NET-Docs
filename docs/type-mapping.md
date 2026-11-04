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
| HUGEINT      | Int128            | BigInteger, UInt128, sbyte, short, int, long, uint, ulong, DuckDBHugeInt
| UTINYINT     | byte              | sbyte, short, int, long, ushort, uint, ulong
| USMALLINT    | ushort            | sbyte, short, int, long, byte, uint, ulong
| UINTEGER     | uint              | sbyte, short, int, long, byte, ushort, ulong
| UBIGINT      | ulong             | sbyte, short, int, long, byte, ushort, uint
| UHUGEINT     | UInt128           | BigInteger, Int128, sbyte, short, int, long, uint, ulong, DuckDBUHugeInt
| REAL         | float             | double, decimal
| DOUBLE       | double            | float, decimal
| DECIMAL      | decimal           | BigInteger, DuckDBDecimal
| UUID         | Guid              | &nbsp;
| VARCHAR      | string            | &nbsp;
| BLOB         | Stream            | &nbsp;
| BIT          | string            | BitArray
| DATE         | DateTime/DateOnly | DateTime, DateOnly, DuckDBDateOnly
| TIME         | TimeOnly          | TimeSpan, DateTime, DuckDBTimeOnly
| TIME_NS      | TimeOnly          | TimeSpan, DateTime, DuckDBTimeOnly
| TIMETZ       | DateTimeOffset    | DuckDBTimeTz
| TIMESTAMP    | DateTime          | DuckDBTimestamp
| TIMESTAMP_NS | DateTime          | DuckDBTimestamp
| TIMESTAMP_MS | DateTime          | DuckDBTimestamp
| TIMESTAMP_S  | DateTime          | DuckDBTimestamp
| TIMESTAMPTZ  | DateTime          | DuckDBTimestamp
| INTERVAL     | TimeSpan          | DuckDBInterval

> [!NOTE]
> A `DECIMAL` value with more significant digits than `System.Decimal` supports (28-29) can't be read as `decimal` — reading it throws `OverflowException`. Such values can still be read losslessly: `GetFieldValue<BigInteger>()` returns the exact value when its fractional part is zero (and throws `InvalidCastException` otherwise), and `GetFieldValue<DuckDBDecimal>()` returns the unscaled value together with the column's width and scale for any `DECIMAL`. `DuckDBDecimal` is also the provider-specific type for `DECIMAL` columns, returned by `GetProviderSpecificValue`.

> [!NOTE]
> `Int128` and `UInt128` are the exact types of `HUGEINT` and `UHUGEINT`, and the default types since DuckDB.NET 2.0 (`BigInteger` before): `GetValue`, `ExecuteScalar`, `GetFieldType` and the schema table use them. Both columns can still be read as `BigInteger` with `GetFieldValue<BigInteger>()`. `VARINT` stays `BigInteger`. The result of `sum()` over an integer column is a `HUGEINT`, so cast the result of `ExecuteScalar` to `Int128`, not `BigInteger`. A `DataTable` or `DataSet` holding such a column can be written with `WriteXml`, but `ReadXml` refuses `Int128` and `UInt128` unless they are added to the allowed types of `DataSet` deserialization (https://go.microsoft.com/fwlink/?linkid=2132227). Every integer column, `VARINT`, `ENUM` and a `DECIMAL` with no fractional part can also be read as `Int128` or `UInt128` when the value fits; otherwise the read throws `InvalidCastException`. `REAL` and `DOUBLE` can't be read as either type. Both types can also be appended (`AppendValue(Int128?)`, `AppendValue(UInt128?)`), used as parameters (sent as `HUGEINT` and `UHUGEINT` values) and used as the parameter and result types of scalar and table functions.

> [!NOTE]
> A `DOUBLE` read as `float` is narrowed like a C# cast: digits are lost, and a value too large for `float` becomes infinity. A `REAL` or `DOUBLE` read as `decimal` is converted with a check: NaN, infinity and values outside the range of `decimal` throw `InvalidCastException`.

> [!NOTE]
> A `TIMETZ` has no date. Read as `DateTimeOffset`, it is placed on 0001-01-02 so that a time earlier in the day than its positive offset can be represented.

> [!NOTE]
> A `TIME_NS` keeps nanoseconds in DuckDB. `TimeOnly`, `DateTime`, and `TimeSpan` expose 100-nanosecond ticks; `DuckDBTimeOnly` exposes whole microseconds.

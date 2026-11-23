---
description: "Mappings between DuckDB column types and .NET types when reading data with GetValue and GetFieldValue<T>."
---

# DuckDB.NET Type Mappings

The following table lists the mappings between DuckDB types and .NET types. The default type is returned by the [`GetValue`](xref:DuckDB.NET.Data.DuckDBDataReader.GetValue(System.Int32)) method. Non-default types can be read by calling the [`GetFieldValue<T>`](xref:DuckDB.NET.Data.DuckDBDataReader.GetFieldValue``1(System.Int32)) method. Calling `GetFieldValue<object>` returns the same value as `GetValue`.

The same mappings apply to list items, map keys and values, struct fields and the arguments of scalar and table functions. A value read into an `object` property of a class or struct has the default type of its column too.

In the table, the integer types are `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `Int128` and `UInt128`.

| DuckDB type  | Default .NET type | Non-default .NET types
| -----------  | ------------      | -----------
| Boolean      | bool              | &nbsp;
| TINYINT      | sbyte             | other integer types, float, double, decimal, BigInteger
| SMALLINT     | short             | other integer types, float, double, decimal, BigInteger
| INTEGER      | int               | other integer types, float, double, decimal, BigInteger
| BIGINT       | long              | other integer types, float, double, decimal, BigInteger
| HUGEINT      | Int128            | other integer types, BigInteger, DuckDBHugeInt
| UTINYINT     | byte              | other integer types, float, double, decimal, BigInteger
| USMALLINT    | ushort            | other integer types, float, double, decimal, BigInteger
| UINTEGER     | uint              | other integer types, float, double, decimal, BigInteger
| UBIGINT      | ulong             | other integer types, float, double, decimal, BigInteger
| UHUGEINT     | UInt128           | other integer types, BigInteger, DuckDBUHugeInt
| VARINT       | BigInteger        | integer types
| REAL         | float             | double, decimal, integer types except Int128 and UInt128
| DOUBLE       | double            | float, decimal, integer types except Int128 and UInt128
| DECIMAL      | decimal           | integer types, BigInteger, DuckDBDecimal
| UUID         | Guid              | &nbsp;
| VARCHAR      | string            | &nbsp;
| BLOB         | byte[]            | Stream
| BIT          | string            | BitArray
| DATE         | DateOnly          | DateTime, DuckDBDateOnly
| TIME         | TimeOnly          | TimeSpan, DateTime, DuckDBTimeOnly
| TIME_NS      | TimeOnly          | TimeSpan, DateTime, DuckDBTimeOnly
| TIMETZ       | DateTimeOffset    | DuckDBTimeTz
| TIMESTAMP    | DateTime          | DateTimeOffset, DuckDBTimestamp
| TIMESTAMP_NS | DateTime          | DateTimeOffset, DuckDBTimestamp
| TIMESTAMP_MS | DateTime          | DateTimeOffset, DuckDBTimestamp
| TIMESTAMP_S  | DateTime          | DateTimeOffset, DuckDBTimestamp
| TIMESTAMPTZ  | DateTime          | DateTimeOffset, DuckDBTimestamp
| INTERVAL     | TimeSpan          | DuckDBInterval

## Numbers

A number read as another numeric type must fit that type, or the read throws `InvalidCastException`. A `REAL`, `DOUBLE` or `DECIMAL` read as an integer type must also be a whole number, so 42.0 read as `int` is 42 and 1.5 throws. A `DOUBLE` read as `float` is narrowed like a C# cast, and a `REAL` or `DOUBLE` read as `decimal` throws for NaN, infinity and values outside the range of `decimal`.

A `DECIMAL` read as `decimal` keeps the scale of its column, so 1.5 from a `DECIMAL(38,4)` column reads as 1.5000. A value with more digits than `decimal` holds (28-29) throws `OverflowException`. You can read it as `BigInteger` if it is a whole number, or as `DuckDBDecimal`, which holds the unscaled value with the column's width and scale.

The result of `sum()` over an integer column is a `HUGEINT`, so cast the result of `ExecuteScalar` to `Int128`. `ReadXml` refuses a `DataTable` with an `Int128` or `UInt128` column unless the types are added to the [allowed types of DataSet deserialization](https://go.microsoft.com/fwlink/?linkid=2132227).

## Dates and times

A timestamp read as `DateTimeOffset` has an offset of zero. Read as `DateTime`, it has `Kind` set to `Unspecified`, also for a `TIMESTAMPTZ`, which DuckDB stores in UTC. The .NET types count in 100-nanosecond ticks, so a `TIMESTAMP_NS` or `TIME_NS` read as one of them loses the rest of its nanoseconds. `DuckDBTimestamp` and `DuckDBTimeOnly` keep whole microseconds.

A `TIMETZ` has no date, so as a `DateTimeOffset` it is placed on 0001-01-02. That way a time earlier in the day than its positive offset can still be represented.

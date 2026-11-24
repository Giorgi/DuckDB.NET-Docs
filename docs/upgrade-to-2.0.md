---
description: "What to change in your code when you move from DuckDB.NET 1.x to 2.0: default types, stricter conversions, parameters, appenders, functions and the low-level bindings."
---

# Upgrading from 1.x to 2.0

DuckDB.NET 2.0 is built on DuckDB 2.0 and contains changes that can affect existing code. This page lists them by area, so you can read the parts your code uses. The [release notes](release-notes/2.0.md) list every change, including the new features.

## Upgrade both packages together

DuckDB.NET.Data 2.0 requires DuckDB.NET.Bindings 2.0, and Data 1.x does not work with Bindings 2.0. If your project references DuckDB.NET.Bindings directly, update it together with DuckDB.NET.Data (or DuckDB.NET.Data.Full).

## HUGEINT and UHUGEINT are read as Int128 and UInt128

In 1.x, a HUGEINT or UHUGEINT value was mapped to a `BigInteger`. In 2.0 it is mapped to `Int128` or `UInt128`, the exact .NET types of these columns. This applies to `GetValue`, `ExecuteScalar`, `GetFieldType`, the schema tables, `DataTable`, and values inside lists, maps and structs.

DuckDB returns the sum of an integer column as a HUGEINT, so casting the result of a `sum()` to `BigInteger` now throws `InvalidCastException`. Cast it to `Int128` instead, like this:

```csharp
command.CommandText = "SELECT sum(quantity) FROM orders";
var total = (Int128)command.ExecuteScalar()!;
```

If you want to keep `BigInteger`, read the value with `GetFieldValue<BigInteger>()`, which still works. VARINT has no fixed size and stays `BigInteger`.

A `DataTable` with an `Int128` or `UInt128` column can be saved with `WriteXml`, but `ReadXml` refuses to load it until you add both types to the [types allowed for DataSet deserialization](https://go.microsoft.com/fwlink/?linkid=2132227).

## BLOB is read as byte[]

`GetValue`, `ExecuteScalar`, `GetProviderSpecificValue` and a read into `object` now give a `byte[]` for a BLOB, and `GetFieldType` and the schema tables report `byte[]`. In 1.x they gave a `Stream`. Code that casts the value to `Stream` now throws. If you need a stream, call `GetStream` or `GetFieldValue<Stream>()`, which still return one.

## Fractions are no longer rounded to integers

When you read a FLOAT or DOUBLE value as an integer type, 1.x rounded it (half to even, so 2.5 became 2). 2.0 throws `InvalidCastException` when the value has a fractional part, and the same applies to DECIMAL. A whole value that fits still reads, and a whole DECIMAL can now be read as an integer type too, which 1.x refused.

If your code relied on the rounding, round in SQL with `round(price)::INTEGER`, or read the value as `double` or `decimal` and round it in C#.

## Wide decimals keep their scale

A DECIMAL with more than 18 digits that fits in a `decimal` now keeps the scale of its column. 1.5 stored in a DECIMAL(38,4) column reads as `1.5000`, as it already did from a DECIMAL(18,4) column. Equality and arithmetic give the same results as before, but `ToString()` and serialized output now show the trailing zeros. Check tests that compare text and any place that shows such values to users.

## Lists, maps and structs

A list item, a map key or value and a struct field are now converted by the same rules as a column, and some reads that failed in 1.x now work. You can read an integer list as `List<BigInteger>`, a HUGEINT list as `List<long>` when the values fit, or an ENUM field as an integer type. A read that cannot work throws `InvalidCastException`. Most of these reads threw `InvalidCastException` in 1.x too, but some threw `ArgumentException`, so check any `catch` blocks that expect it.

The type you read a LIST or MAP into must be one the reader can create and fill. An interface or abstract class such as `IEnumerable<int>`, `IList<int>` or `IReadOnlyDictionary<string, int>`, a `HashSet<int>`, or a `List<KeyValuePair<string, int>>` for a map is refused with `InvalidCastException`. In 1.x these failed with `MissingMethodException`, `ArgumentException` or `InvalidOperationException`. Read into `List<T>` or `Dictionary<TKey, TValue>`, or into a class derived from them, which now works.

A class derived from `Dictionary<string, object>` is refused as a MAP target, as `Dictionary<string, object>` itself already was.

## Values read into object

A value read into `object` now has the same .NET type that `GetValue` gives for its column. This applies to an `object` property of a class or struct and to `GetValue<object>` in a function. In 1.x a DATE, TIMESTAMP, TIME, TIMETZ or INTERVAL value read into an `object` property or with `GetValue<object>` in a scalar function was a DuckDB struct, so a check such as `value is DuckDBDateOnly` no longer matches. Numbers, HUGEINT, ENUM, STRUCT, LIST and MAP values threw or gave an empty object.

## DuckDBDateOnly and DuckDBTimeOnly convert explicitly

In 1.x `DuckDBDateOnly` converted to `DateOnly`, and `DuckDBTimeOnly` to `TimeOnly`, without a cast. In 2.0 these conversions are explicit. An assignment that relied on the implicit conversion stops compiling, and adding the cast fixes it.

Two calls still compile but behave differently. `Equals` now picks the `Equals(object)` overload and returns `false`, and `CompareTo` throws `ArgumentException`. Cast the DuckDB value first, like this:

```csharp
var fromDuckDB = reader.GetFieldValue<DuckDBDateOnly>(0);
var date = new DateOnly(2026, 11, 20);

var same = date.Equals((DateOnly)fromDuckDB);
var order = date.CompareTo((DateOnly)fromDuckDB);
```

An assembly that was compiled against 1.x and uses one of these conversions fails at run time until you rebuild it.

## Parameters

DuckDB.NET 1.x converted a parameter value with `Convert.ChangeType`, and sent the value's `ToString()` in the current culture when that failed. A `DbType` set before `Value` was lost when `Value` was assigned. The sections below describe how 2.0 converts parameter values.

### A DbType you set decides the value

A `DbType` stays on the parameter whether you set it before `Value`, after it, or in the constructor ([#369](https://github.com/Giorgi/DuckDB.NET/issues/369), by Alaric Dailey). When the command runs, the value is converted to the .NET type of that `DbType`. A value that doesn't fit that type, or has a fraction for an integer `DbType`, is refused with an `InvalidCastException`. So `DbType.Int64` takes an `int` and `DbType.Decimal` an integer or a `double`. `DbType.Date` takes a `DateTime` and keeps its date, and `DbType.Time` takes a `TimeSpan` shorter than a day. `DbType.Single` and `DbType.Double` take any number, and one too large becomes infinity. A `double` or `float` with `DbType.Decimal` keeps 15 or 7 significant digits, as a C# cast does. `DbType.Int32` with 2.5, `DbType.Int32` with `"42"`, `DbType.String` with 42 and `DbType.Boolean` with 1 are all refused, because a `DbType` never parses text and never turns a number into text. The message names the parameter and both types, like this:

```
Cannot cast from Double to Int32 for parameter "id" (DbType.Int32): the value has a non-zero fractional part
```

If you set a `DbType` only because an API asked for one, call `ResetDbType()` or leave it out, and the value is sent as its own type. A new parameter now reports `DbType.Object` (it was `String`), `ResetDbType()` goes back to the type of the current value, and a `decimal` value gives `DbType.Decimal` (it was `Currency`, [#382](https://github.com/Giorgi/DuckDB.NET/issues/382) by Alaric Dailey).

A `TimeSpan` with `DbType.Time` is a time of day, so a negative value or one of a day or more throws `InvalidCastException`. In 1.x it threw `ArgumentOutOfRangeException`. An INTERVAL parameter still takes any `TimeSpan`.

### Without a DbType the value is sent as its own type

DuckDB can often tell a parameter's type, for example from the column it is compared with. If that type is an integer type, a value of any .NET integer type is converted to it when it fits, so a `long` 5 for an INTEGER column is sent as an INTEGER. A `decimal` for a DECIMAL column is sent with the column's width and scale when it fits them. Any other value is sent as its own type and DuckDB casts it as it would cast a SQL literal. That cast rounds a decimal half away from zero, so a `decimal` 2.5 for an INTEGER parameter is now 3 where 1.x sent 2. A `double` 2.5 is still 2, because DuckDB rounds a DOUBLE half to even.

A few other cases change with this. A `char` for an integer parameter is sent as text, so `'5'` gives 5 and `'A'` is refused, where 1.x sent the character codes 53 and 65. A fraction in a comparison is no longer rounded before DuckDB sees it, so `WHERE id = $1` with 1.5 matches nothing. A value too large for its parameter is refused by DuckDB with its own range message. A `double`, `float`, `decimal` or `bool` passed to a function argument that DuckDB types as an integer no longer binds, so `substring('abcdef', $1, 2)` with 3.0 fails with "No function matches". Pass an integer there, or a string such as `"3"`.

An `Int128` or `UInt128` parameter is now sent as a HUGEINT or UHUGEINT value, with `DbType.VarNumeric`. In 1.x it was sent as text.

### Nothing depends on the current culture

A value that has no DuckDB type of its own is sent as text formatted with the invariant culture. For example, a `Half` 1.5 was sent as `1,5` on a machine with a German culture, and is now sent as `1.5` on every machine. List items without a `DbType` are sent as their own type too. A `DateTime` in a `VARCHAR[]` parameter is now stored as `2026-11-19 13:45:30` in any culture. A `DateTimeOffset` list item comes out in UTC, while a single `DateTimeOffset` parameter is cast in the session's time zone.

### Dapper

Dapper gives most parameters the `DbType` of their .NET type, so their values arrive as before. `DateTime`, `DateOnly`, `TimeOnly` and `TimeSpan` get no `DbType` from Dapper. The one change is that `DynamicParameters.Add(name, value, dbType)` with a value that does not fit the `DbType`, such as 2.5 with `DbType.Int32`, is now refused.

## Prepared commands

In 1.x `DuckDBCommand.Prepare()` did nothing. In 2.0 it prepares the statement and keeps it on the connection until the connection closes or the same text is prepared again. Every prepared command on that connection with the same text uses the newest statement. Later executions skip parsing and preparing the statement. If you change the schema in a way that changes the type of a parameter, for example by altering a column, call `Prepare()` again. A command that holds several statements is not prepared. `Prepare()` now needs an open connection and throws `InvalidOperationException` when the connection is closed ([#378](https://github.com/Giorgi/DuckDB.NET/issues/378), by Alaric Dailey).

## ADO.NET behaviour

Several members now behave as ADO.NET documents them. These changes come from pull requests by Alaric Dailey.

`ExecuteNonQuery` now returns -1 for statements other than INSERT, UPDATE, DELETE, MERGE and COPY, where it returned 0 ([#374](https://github.com/Giorgi/DuckDB.NET/issues/374)). If your code checks the result of a CREATE TABLE or a SET statement, it now gets -1. `RecordsAffected` on a data reader was always -1 in 1.x, and now gives the same count as `ExecuteNonQuery`.

`DbCommand.CommandType` now starts as `Text` and `DbParameter.Direction` as `Input` ([#372](https://github.com/Giorgi/DuckDB.NET/issues/372)). Both started at 0, which is not a valid value of either enum.

`GetOrdinal` now tries a case-insensitive match when no column has the exact name, and throws `IndexOutOfRangeException` when it finds none ([#375](https://github.com/Giorgi/DuckDB.NET/issues/375)). In 1.x it threw `DuckDBException`.

DuckDB.NET does not support savepoints, and `SupportsSavepoints` on a transaction returns `false`. `Save` and `Rollback` with a savepoint name already threw `NotSupportedException` in 1.x, but `Release` and `ReleaseAsync` did nothing. They now throw `NotSupportedException` as well ([#380](https://github.com/Giorgi/DuckDB.NET/issues/380)).

## Appender

`IDuckDBAppenderRow` has new members `AppendValue(Int128?)`, `AppendValue(UInt128?)`, `AppendValue(DateOnly?)` and `AppendValue(TimeOnly?)`. A class that implements the interface must add them.

The new overloads make an integer constant ambiguous in C# 12, the default language version for net8.0 projects. `row.AppendValue(1)` fails with CS0121 there. Newer C# versions compile it, but bind it to a one-byte overload, as 1.x did, and one byte written into an INTEGER column stores a wrong value. The appender writes the bytes of the .NET type it is given, so give every value the type of its column, like this:

```csharp
appender.AppendRow(row => row.AppendValue((int?)1).AppendValue((long?)2));
```

A `BigInteger` appended to a column other than HUGEINT or UHUGEINT now throws `InvalidOperationException`, as an `Int128` does. In 1.x it overwrote the values next to it.

A `DateTime` appended to a TIME column now stores its time of day. In 1.x it stored a wrong value. A `TimeOnly` or `DuckDBTimeOnly` written to a DATE, TIMETZ or timestamp column now throws `InvalidOperationException`, and so does a `DateOnly` or `DuckDBDateOnly` written to a TIME, TIMETZ or timestamp column. A `DateTime` written to a TIMETZ column throws too. In 1.x all of these stored wrong values. This applies to the appender and to the results of scalar and table functions.

A string that contains a NUL character is now stored in full by the appender, the mapped appender, scalar and table function results, and parameters. 1.x cut it at the first NUL.

## Scalar functions

A typed scalar function with no nullable parameter now returns NULL for a NULL argument without calling your function, for a column as well as for a constant. In 1.x a value-type parameter threw "Column '' value is null" for a NULL in a column, and a `string` or `object` parameter received `null`. In a project without nullable reference types, `string` and `object` count as not nullable too, so if your function handles `null` itself, declare the parameter `string?` or mark it `[AllowNull]` or `[MaybeNull]`. See [NULL Handling](scalar-functions.md#null-handling).

DuckDB.NET finds out whether a parameter is nullable from the annotations of the delegate's method. If you register the function from a generic method and its parameter type is a type parameter `T`, calling that method with `string?` as `T` leaves no annotation in the compiled code. Such a parameter is treated as not nullable, and the function is not called for NULL rows.

## Table functions

`CardinalityHint` tells DuckDB how many rows a table function returns, and DuckDB 2.0 corrected what its `IsExact` flag means. With DuckDB 1.5, `false` made the number the most rows the function can return and `true` made it an estimate. Now `false` is an estimate, and `true` is an exact count that DuckDB also treats as a maximum. If you passed `true` for a guess and the function returns more rows, a query can give a wrong sum or ignore a LIMIT, so pass `false` for a guess.

A table function parameter read with `GetValue<T>` must be read as the .NET type of its DuckDB type, as `object`, as a nullable of it or as an interface it implements. A HUGEINT can also be read as `BigInteger`, a DATE as `DuckDBDateOnly`, a TIME as `DuckDBTimeOnly` and a timestamp as `DateTimeOffset`. Any other `T` throws `InvalidCastException`. In 1.x such a read gave a wrong value. Check `IsNull()` before you read a parameter, because `GetValue` doesn't handle a NULL argument.

## Low-level bindings

These changes matter only if you call DuckDB.NET.Bindings directly.

`DuckDBString.value` and the nested type `DuckDBString._value_e__Union` are removed. Read a string with `Length` and `Data`, and build a short one with `DuckDBString.TryCreateInlined`. `DuckDBStringInlined`, which was nested in the removed union, is now internal.

`NativeMethods.Value.DuckDBCreateVarchar` takes a non-nullable string, and passing `null` crashes the process. Use `DuckDBCreateNullValue` for NULL. A new overload, `DuckDBCreateVarchar(ReadOnlySpan<byte>)`, takes UTF-8 bytes and keeps NUL characters. Because of it, `DuckDBCreateVarchar(null)` and `DuckDBCreateVarchar(default)` are ambiguous in C# 12, and a method group or a reflection lookup of `DuckDBCreateVarchar` needs the parameter type.

`DuckDBValue.GetValue<T>` gives the value as its own type, as `object`, as a nullable of it or as an interface it implements. Any other `T` throws `InvalidCastException`. In 1.x it reinterpreted the value's memory as `T`.

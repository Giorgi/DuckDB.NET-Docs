---
description: "Bulk load .NET objects into DuckDB with the type-safe Mapped Appender: declarative property-to-column mappings with automatic type validation."
---

# Mapped Appender

The Mapped Appender provides a type-safe way to bulk load data from .NET objects into DuckDB tables. It uses [`DuckDBAppenderMap`](xref:DuckDB.NET.Data.Mapping.DuckDBAppenderMap`1) to define mappings between your object properties and table columns, with automatic type validation.

## Overview

While the standard [DuckDBAppender](xref:DuckDB.NET.Data.DuckDBAppender) provides efficient bulk loading, it requires manual column ordering and type management. The Mapped Appender ([`DuckDBMappedAppender<T, TMap>`](xref:DuckDB.NET.Data.DuckDBMappedAppender`2)) adds:

- **Type Safety**: Compile-time type checking for property mappings
- **Automatic Validation**: Runtime verification that mapped types match table schema
- **Simplified API**: Map properties declaratively instead of manual value appending
- **Better Maintainability**: Clear mapping definitions separate from business logic

## Basic Usage

To use a mapped appender, you need to:

1. Define your data class
2. Create an `AppenderMap` class that defines how properties map to columns
3. Use [`CreateAppender<T, TMap>()`](xref:DuckDB.NET.Data.DuckDBConnection.CreateAppender*) to create a type-safe appender
4. Call [`AppendRecords()`](xref:DuckDB.NET.Data.DuckDBMappedAppender`2.AppendRecords*) to insert your data

### Example

[!code-csharp[](../code/MappedAppenderExample.cs)]

## Creating AppenderMap Classes

An `AppenderMap` class inherits from [DuckDBAppenderMap&lt;T&gt;](xref:DuckDB.NET.Data.Mapping.DuckDBAppenderMap`1) and defines the mapping in its constructor using these methods:

### Map(Func&lt;T, TProperty&gt; getter)

Maps a property to the next column in sequence. The lambda expression extracts the property value from your object.

[!code-csharp[](../code/MappedAppenderMapClass.cs#Example)]

> [!IMPORTANT]
> Mappings must be defined in the **exact same order** as the table columns. The first `Map()` call maps to the first column, the second to the second column, etc.

### DefaultValue()

Uses the column's default value (defined in the table schema) for the next column.

[!code-csharp[](../code/MappedAppenderDefaultValue.cs#Example)]

### NullValue()

Inserts a NULL value for the next column.

[!code-csharp[](../code/MappedAppenderNullValue.cs#Example)]

## Type Validation

The mapped appender validates that your mapped .NET types match the DuckDB column types when the appender is created. This catches type mismatches early, before any data is written.

### Supported Type Mappings

The following .NET types are supported for mapping:

| .NET Type | DuckDB Type |
|-----------|-------------|
| `bool` | BOOLEAN |
| `sbyte` | TINYINT |
| `short` | SMALLINT |
| `int` | INTEGER |
| `long` | BIGINT |
| `byte` | UTINYINT |
| `ushort` | USMALLINT |
| `uint` | UINTEGER |
| `ulong` | UBIGINT |
| `float` | REAL/FLOAT |
| `double` | DOUBLE |
| `decimal` | DECIMAL |
| `string` | VARCHAR/TEXT |
| `DateTime` | TIMESTAMP |
| `DateTimeOffset` | TIMESTAMPTZ |
| `TimeSpan` | INTERVAL |
| `Guid` | UUID |
| `BigInteger` | HUGEINT |
| `DateOnly` (.NET 6+) | DATE |
| `TimeOnly` (.NET 6+) | TIME |
| [`DuckDBDateOnly`](xref:DuckDB.NET.Native.DuckDBDateOnly) | DATE |
| [`DuckDBTimeOnly`](xref:DuckDB.NET.Native.DuckDBTimeOnly) | TIME |

Nullable versions of these types (e.g., `int?`, `DateTime?`) are also supported and will write NULL when the value is null.

### Type Mismatch Example

[!code-csharp[](../code/MappedAppenderTypeMismatch.cs#Map)]
[!code-csharp[](../code/MappedAppenderTypeMismatch.cs#Usage)]

The error message will indicate exactly which column has the type mismatch:
```
Type mismatch at column index 1: Mapped type is DateTime (expected DuckDB type: Timestamp) but actual column type is Real
```

## API Reference

### CreateAppender Methods

The [DuckDBConnection](xref:DuckDB.NET.Data.DuckDBConnection) class provides three overloads:

[!code-csharp[](../code/MappedAppenderCreateOverloads.cs)]

### DuckDBMappedAppender&lt;T, TMap&gt; Methods

#### AppendRecords(IEnumerable&lt;T&gt; records)

Appends multiple records to the table.

[!code-csharp[](../code/MappedAppenderAppendRecords.cs#Example)]

> [!NOTE]
> If appending a record fails partway through `AppendRecords` - for example, a property getter throws - that record is discarded and no further records can be appended. Records completed before the failure are written when the appender is closed or disposed. Because DuckDB flushes in batches, the number of records already persisted is not defined; wrap the call in a transaction and roll back on failure if you need all-or-nothing.

#### Close()

Closes the appender and flushes any remaining data to the database. This is called automatically when disposing.

```csharp
appender.Close();
```

#### Dispose()

Disposes the appender and releases resources. Always dispose appenders to ensure data is flushed.

[!code-csharp[](../code/MappedAppenderDispose.cs#Example)]

## Complete Example with Default and Null Values

[!code-csharp[](../code/MappedAppenderAdvanced.cs)]

## Performance Considerations

- The mapped appender has a tiny overhead compared to the raw appender due to type validation and property accessor invocation
- Type validation occurs once when the appender is created, not for each record
- For maximum performance with tens of millions of rows, consider the raw [DuckDBAppender](xref:DuckDB.NET.Data.DuckDBAppender)
- For most use cases, the performance difference is negligible and the type safety is worth it

# User-Defined Functions in DuckDB.NET

DuckDB.NET supports creating user-defined functions with C# for extending the functionality of DuckDB.

## Scalar User-Defined Functions

Scalar functions return one value per invocation. They can be used to perform some calculation on the input parameters and return a single result.

DuckDB.NET 1.5.0 added a high-level API that lets you register a scalar function with a plain `Func<>` delegate, with automatic NULL handling inferred from nullable parameter types. A low-level vector-based API is also available for direct control over chunk processing.

## Table User-Defined Functions

Table-valued functions return collections of rows and can be called anywhere in SQL where you can use a table. In DuckDB they are mostly used to access external data for further data processing.

DuckDB.NET 1.5.0 added a high-level API that uses projection expressions to define columns and mapping in one place, with support for named parameters via the `[Named]` attribute. A low-level API is also available for full control over column definitions and the mapper callback.

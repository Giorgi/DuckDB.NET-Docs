# User-Defined Functions in DuckDB.NET

DuckDB.NET supports creating user-defined functions with C# for extending the functionality of DuckDB.

## Scalar User-Defined Functions

Support added in DuckDB.NET 1.1.

Scalar functions return one value per invocation. They can be used to perform some calculation on the input parameters and return a single result.

## Table User-Defined Functions

Support added in DuckDB.NET 1.1.3.

Table-valued functions return collections of rows and can be called anywhere in SQL where you can use a table. In DuckDB they are mostly used to access external data for further data processing.

# User-Defined Functions in DuckDB.NET

DuckDB supports creating user-defined functions for expending the functionality of DuckDB. Starting from DuckDB.NET 1.1,
you can create scalar UDFs with C#. Table valued functions are supported from DuckDB.NET 1.1.3

## Scalar User-Defined Functions
Scalar functions is a function that returns one value per invocation. They can be used to perform some calculation on the input parameters and return a single result.

## Table User-Defined Functions
Table-valued functions return collections of rows and can be called anywhere in SQL where you can use a table. In DuckDB they are mostly used to access external data for further data processing.
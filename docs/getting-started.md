# Getting Started

## ADO.NET Provider and Low-Level Library

There are two ways to work with DuckDB from C#: You can use ADO.NET Provider or use a low-level bindings library for DuckDB. The ADO.NET Provider is built on top of the low-level library and is the recommended and most straightforward approach to work with DuckDB.

In both cases, two [NuGet packages](https://www.nuget.org/packages?q=Tags%3A%22DuckDB%22+Author%3A%22Giorgi%22&includeComputedFrameworks=true&prerel=true&sortby=relevance) are available: The Full package that includes the DuckDB native library and a managed-only library that doesn't include a native library.

|  | ADO.NET Provider | Includes DuckDB library |
|---|---|---|
| DuckDB.NET.Bindings | :x: | :x: |
| DuckDB.NET.Bindings.**Full** | :x: | :white_check_mark: |
| DuckDB.NET.Data | :white_check_mark: | :x: |
| DuckDB.NET.Data.**Full** | :white_check_mark: | :white_check_mark: |

## Using ADO.NET Provider

```sh
dotnet add package DuckDB.NET.Data.Full
```

DuckDB.NET follows the ADO.NET provider model and its API.
The following snippet shows how to run basic operations:

[!code-csharp[](../code/GettingStarted.cs)]

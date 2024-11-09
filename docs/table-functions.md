# Table Functions

To register a table function, call one of the [RegisterTableFunction](xref:DuckDB.NET.Data.DuckDBConnection.RegisterTableFunction*) overloads specifying the function name, input parameter type(s), callback for returning result set and a callback function that maps an item from the result map. 

```cs
connection.RegisterTableFunction<string, int>("github_search", (parameters) =>
{
    var term = parameters[0].GetValue<string>();
    var stars = parameters[1].GetValue<int>();

    var client = new GitHubClient(new ProductHeaderValue("DuckDB-Table-Valued-Function"));

    var request = new SearchRepositoriesRequest(term)
    {
        Stars = new Octokit.Range(stars, SearchQualifierOperator.GreaterThan)
    };

    var result = client.Search.SearchRepo(request).ConfigureAwait(false).GetAwaiter().GetResult();

    return new TableFunction(new List<ColumnInfo>()
    {
        new ColumnInfo("name", typeof(string)),
        new ColumnInfo("description", typeof(string)),
        new ColumnInfo("stargazers", typeof(int)),
        new ColumnInfo("url", typeof(string)),
        new ColumnInfo("owner", typeof(string)),
    }, result.Items);
}, (item, writers, rowIndex) =>
{
    var repo = (Repository)item;
    writers[0].WriteValue(repo.Name, rowIndex);
    writers[1].WriteValue(repo.Description, rowIndex);
    writers[2].WriteValue(repo.StargazersCount, rowIndex);
    writers[3].WriteValue(repo.Url, rowIndex);
    writers[4].WriteValue(repo.Owner.Login, rowIndex);
});
```

In the example above we define a table function, `github_search` that takes two parameters: a `string` parameter that is used as a search term and an `int` that filters results by stargazers. The first callback, `resultCallback`, provides access to the parameters that the functions was invoked with and returns a `TableFunction` instance that describes the columns that our table function has and an `IEnumerable<>` that serves as a result set of the invocation. The second callback, `mapperCallback` will be invoked by DuckDB.NET for every item in the result that. In this callback you should use the provided `IDuckDBDataWriter` array to write the item back to DuckDB.

After registering the table function you can use just like any other table. For example, you can search for all DuckDB repositories with 400 stars or more
with the following query:

```cs
var data = connection.Query<(string, string, int, string, string)>("SELECT * FROM github_search('duckdb', 400);");
```

Executing the above query produces the following result:

 | name            | description                                                                                                                                                                                                                                                             | stargazers | url                                                         | owner                   |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------:| ----------------------------------------------------------- | ----------------------- |
| duckdb          | DuckDB is an analytical in-process SQL database management system                                                                                                                                                                                                       | 24115      | https://api.github.com/repos/duckdb/duckdb                  | duckdb                  |
| duckdb-wasm     | WebAssembly version of DuckDB                                                                                                                                                                                                                                           | 1272       | https://api.github.com/repos/duckdb/duckdb-wasm             | duckdb                  |
| dbt-duckdb      | dbt (http://getdbt.com) adapter for DuckDB (http://duckdb.org)                                                                                                                                                                                                          | 916        | https://api.github.com/repos/duckdb/dbt-duckdb              | duckdb                  |
| awesome-duckdb  | 🦆 A curated list of awesome DuckDB resources                                                                                                                                                                                                                           | 1358       | https://api.github.com/repos/davidgasquez/awesome-duckdb    | davidgasquez            |
| go-duckdb       | go-duckdb provides a database/sql driver for the DuckDB database engine.                                                                                                                                                                                                | 716        | https://api.github.com/repos/marcboeker/go-duckdb           | marcboeker              |
| duckdb-rs       |  Ergonomic bindings to duckdb for Rust                                                                                                                                                                                                                                  | 501        | https://api.github.com/repos/duckdb/duckdb-rs               | duckdb                  |
| pg_duckdb       | DuckDB-powered Postgres for high performance apps & analytics.                                                                                                                                                                                                          | 1575       | https://api.github.com/repos/duckdb/pg_duckdb               | duckdb                  |
| DuckDB.NET      | Bindings and ADO.NET Provider for DuckDB                                                                                                                                                                                                                                | 430        | https://api.github.com/repos/Giorgi/DuckDB.NET              | Giorgi                  |
| duckdb_spatial  |                                                                                                                                                                                                                                                                         | 483        | https://api.github.com/repos/duckdb/duckdb_spatial          | duckdb                  |
| sql-studio      | SQL Database Explorer [SQLite, libSQL, PostgreSQL, MySQL/MariaDB, DuckDB, ClickHouse]                                                                                                                                                                                   | 2330       | https://api.github.com/repos/frectonz/sql-studio            | frectonz                |
| nba-monte-carlo | Monte Carlo simulation of the NBA season, leveraging dbt, duckdb and evidence.dev                                                                                                                                                                                       | 446        | https://api.github.com/repos/matsonj/nba-monte-carlo        | matsonj                 |
| logica          | Logica is a logic programming language that compiles to SQL. It runs on DuckDB, Google BigQuery, PostgreSQL and SQLite.                                                                                                                                                 | 1767       | https://api.github.com/repos/EvgSkv/logica                  | EvgSkv                  |
| WhatTheDuck     | WhatTheDuck is an open-source web application built on DuckDB. It allows users to upload CSV files, store them in tables, and perform SQL queries on the data.                                                                                                          | 515        | https://api.github.com/repos/incentius-foss/WhatTheDuck     | incentius-foss          |
| mosaic          | An extensible framework for linking databases and interactive views.                                                                                                                                                                                                    | 834        | https://api.github.com/repos/uwdata/mosaic                  | uwdata                  |
| lance           | Modern columnar data format for ML and LLMs implemented in Rust. Convert from parquet in 2 lines of code for 100x faster random access, vector index, and data versioning. Compatible with Pandas, DuckDB, Polars, Pyarrow, and PyTorch with more integrations coming.. | 3932       | https://api.github.com/repos/lancedb/lance                  | lancedb                 |
| tad             | A desktop application for viewing and analyzing tabular data                                                                                                                                                                                                            | 3174       | https://api.github.com/repos/antonycourtney/tad             | antonycourtney          |
| narwhals        | Lightweight and extensible compatibility layer between dataframe libraries!                                                                                                                                                                                             | 557        | https://api.github.com/repos/narwhals-dev/narwhals          | narwhals-dev            |
| scratchdata     | Scratch is a swiss army knife for big data.                                                                                                                                                                                                                             | 1098       | https://api.github.com/repos/scratchdata/scratchdata        | scratchdata             |
| ingestr         | ingestr is a CLI tool to copy data between any databases with a single command seamlessly.                                                                                                                                                                              | 2547       | https://api.github.com/repos/bruin-data/ingestr             | bruin-data              |
| splink          | Fast, accurate and scalable probabilistic data linkage with support for multiple SQL backends                                                                                                                                                                           | 1359       | https://api.github.com/repos/moj-analytical-services/splink | moj-analytical-services |
| FreeSql         | 🦄 .NET aot orm, C# orm, VB.NET orm, Mysql orm, Postgresql orm, SqlServer orm, Oracle orm, Sqlite orm, Firebird orm, 达梦 orm, 人大金仓 orm, 神通 orm, 翰高 orm, 南大通用 orm, 虚谷 orm, 国产 orm, Clickhouse orm, DuckDB orm, TDengine orm, QuestDB orm, MsAccess orm.                   | 4113       | https://api.github.com/repos/dotnetcore/FreeSql             | dotnetcore              |
| inline-sql      | 🪄 Inline SQL in any Python program                                                                                                                                                                                                                                     | 418        | https://api.github.com/repos/ekzhang/inline-sql             | ekzhang                 |
| ibis            | the portable Python dataframe library                                                                                                                                                                                                                                   | 5282       | https://api.github.com/repos/ibis-project/ibis              | ibis-project            |
| sqlglot         | Python SQL Parser and Transpiler                                                                                                                                                                                                                                        | 6701       | https://api.github.com/repos/tobymao/sqlglot                | tobymao                 |
| evidence        | Business intelligence as code: build fast, interactive data visualizations in pure SQL and markdown                                                                                                                                                                     | 4392       | https://api.github.com/repos/evidence-dev/evidence          | evidence-dev            |
| qstudio         | qStudio - Free SQL Analysis Tool                                                                                                                                                                                                                                        | 568        | https://api.github.com/repos/timeseries/qstudio             | timeseries              |
| WrenAI          | 🚀 An open-source SQL AI (Text-to-SQL) Agent that empowers data, product teams to chat with their data, and directly connect with Excel and Google Sheets.🤘                                                                                                            | 2016       | https://api.github.com/repos/Canner/WrenAI                  | Canner                  |
| BemiDB          | Postgres read replica optimized for analytics                                                                                                                                                                                                                           | 747        | https://api.github.com/repos/BemiHQ/BemiDB                  | BemiHQ                  |
| pretzelai       | The modern replacement for Jupyter Notebooks                                                                                                                                                                                                                            | 2005       | https://api.github.com/repos/pretzelai/pretzelai            | pretzelai               |
| rill            | Rill is a tool for effortlessly transforming data sets into powerful, opinionated dashboards using SQL.  BI-as-code.                                                                                                                                                    | 1714       | https://api.github.com/repos/rilldata/rill                  | rilldata                |
| latitude        | Developer-first embedded analytics                                                                                                                                                                                                                                      | 877        | https://api.github.com/repos/latitude-dev/latitude          | latitude-dev            |
| vulcan-sql      | Data API Framework for AI Agents and Data Apps                                                                                                                                                                                                                          | 641        | https://api.github.com/repos/Canner/vulcan-sql              | Canner                  |
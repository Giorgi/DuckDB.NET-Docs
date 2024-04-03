# Connection String Parameters

To connect to a DuckDB database, a connection string should be specified. The connection string contains information about the target database and other parameters that control different aspects of the database behavior.

## Basic connection

DuckDB.NET connection strings follow the standard [ADO.NET](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/connection-strings) syntax as a semicolon-separated list of keywords and values. The connection string must specify the database path to the database file with either `DataSource` or `Data Source` keyword. If the specified file does not exist, it will be created the first time the connection is opened.

## In-Memory database

For an in-memory database use `Data Source=:memory:` connection string. When using an in-memory database no data is persisted on disk. Every in-memory connection results in a new, isolated database so tables created inside one in-memory connection aren't visible to another in-memory connection. If you want to create a shared in-memory database, you can use the `Data Source=:memory:?cache=shared` connection string. Both connection strings are exposed by the library as `DuckDBConnectionStringBuilder.InMemoryDataSource` and `DuckDBConnectionStringBuilder.InMemorySharedDataSource` respectively.

## DuckDB configuration

DuckDB has several [configuration options](https://duckdb.org/docs/sql/configuration#configuration-reference) that can be used to change the behavior of the system. These options can be included in the connection string and they will be automatically set when a connection is opened.

## Example connections strings

| Connection String                 | Description |
| -----------                       | ----------- |
| DataSource = :memory:             | Connect to a new in-memory database       |
| DataSource = :memory:?cache=shared| Connect to a shared, in-memory database   |
| DataSource = train_services.db    | Connect to train_services.db              |
| DataSource = train_services.db;ACCESS_MODE=READ_ONLY    | Connect to train_services.db, make connection read-only              |
| DataSource = :memory:;threads=8;ACCESS_MODE=READ_ONLY | Connect to a new in-memory database, limit threads to 8, make connection read-only  |
| DataSource = train_services.db;ACCESS_MODE=READ_ONLY;memory_limit=10GB | Connect to train_services.db, make connection read-only, limit RAM usage to 10GB|

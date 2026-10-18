---
description: "DuckDB.NET connection string syntax: file-backed and in-memory databases, shared caches, MotherDuck, and DuckDB configuration options, including the typed DuckDBConnectionStringBuilder properties."
---

# Connection String Parameters

To connect to a DuckDB database, a connection string should be specified. The connection string contains information about the target database and other parameters that control different aspects of the database behavior.

## Basic connection

DuckDB.NET connection strings follow the standard [ADO.NET](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/connection-strings) syntax as a semicolon-separated list of keywords and values. The connection string must specify the database path to the database file with either `DataSource` or `Data Source` keyword. If the specified file does not exist, it will be created the first time the connection is opened.

## In-Memory database

For an in-memory database use `Data Source=:memory:` connection string. When using an in-memory database no data is persisted on disk. Every in-memory connection results in a new, isolated database so tables created inside one in-memory connection aren't visible to another in-memory connection. If you want to create a shared in-memory database, you can use the `Data Source=:memory:?cache=shared` connection string. Both connection strings are exposed by the library as [`DuckDBConnectionStringBuilder.InMemoryDataSource`](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.InMemoryDataSource) and [`DuckDBConnectionStringBuilder.InMemorySharedDataSource`](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.InMemorySharedDataSource) respectively.

## MotherDuck 

To connect to [MotherDuck](https://motherduck.com):

```cs
using var duckDBConnection = new DuckDBConnection("DataSource=md:{your_database}?motherduck_token=ey...");
```

## DuckDB configuration

DuckDB has many [configuration options](https://duckdb.org/docs/sql/configuration#configuration-reference) that change how it behaves. Add them to the connection string as keywords, and DuckDB.NET sets them when it opens the database. For example, `DataSource=train_services.db;access_mode=READ_ONLY;threads=8` opens the database read-only and limits DuckDB to 8 threads.

- A keyword that isn't a DuckDB configuration option throws an `InvalidOperationException`.
- Some options can't be set when the database opens, and opening the connection fails with an error such as `Error setting 'enable_progress_bar' to 'false'`. Set these options with a [`SET`](https://duckdb.org/docs/sql/statements/set) statement after the connection is open.
- An option that takes a list of values needs the value in DuckDB's list syntax, in double quotes because it contains single quotes: `extension_directories="['/ext1', '/ext2']"`. [DuckDBConnectionStringBuilder](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder) adds the double quotes when you set the value through its indexer.
- `allowed_directories` and `allowed_paths` can't be set in the connection string at all, because of a DuckDB bug ([duckdb/duckdb#25057](https://github.com/duckdb/duckdb/issues/25057)). Set them with `SET` after the connection is open, for example `SET allowed_directories = ['/data'];`.

### Typed properties

Starting with version 1.5.6, [DuckDBConnectionStringBuilder](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder) has typed properties for 24 options that can be set when the database opens. Each property reads and writes the keyword in the table below. Set a property to `null` to leave its option out of the connection string, so that DuckDB's default applies.

[!code-csharp[](../code/ConnectionStringBuilderOptions.cs#Example)]

| Category | Property | Keyword | Type |
| ----------- | ----------- | ----------- | ----------- |
| Data Source | [AccessMode](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.AccessMode) | `access_mode` | [DuckDBAccessMode](xref:DuckDB.NET.Data.DuckDBAccessMode) |
| Resources | [Threads](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.Threads) | `threads` | `int` |
| Resources | [ExternalThreads](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.ExternalThreads) | `external_threads` | `int` |
| Resources | [MemoryLimit](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.MemoryLimit) | `memory_limit` | `string` |
| Resources | [TempDirectory](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.TempDirectory) | `temp_directory` | `string` |
| Resources | [MaxTempDirectorySize](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.MaxTempDirectorySize) | `max_temp_directory_size` | `string` |
| Storage | [CheckpointThreshold](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.CheckpointThreshold) | `checkpoint_threshold` | `string` |
| Storage | [StorageCompatibilityVersion](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.StorageCompatibilityVersion) | `storage_compatibility_version` | `string` |
| Security | [EnableExternalAccess](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.EnableExternalAccess) | `enable_external_access` | `bool` |
| Security | [AllowUnsignedExtensions](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.AllowUnsignedExtensions) | `allow_unsigned_extensions` | `bool` |
| Security | [AllowCommunityExtensions](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.AllowCommunityExtensions) | `allow_community_extensions` | `bool` |
| Security | [AllowPersistentSecrets](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.AllowPersistentSecrets) | `allow_persistent_secrets` | `bool` |
| Security | [SecretDirectory](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.SecretDirectory) | `secret_directory` | `string` |
| Security | [LockConfiguration](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.LockConfiguration) | `lock_configuration` | `bool` |
| Extensions | [AutoloadKnownExtensions](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.AutoloadKnownExtensions) | `autoload_known_extensions` | `bool` |
| Extensions | [AutoinstallKnownExtensions](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.AutoinstallKnownExtensions) | `autoinstall_known_extensions` | `bool` |
| Extensions | [ExtensionDirectory](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.ExtensionDirectory) | `extension_directory` | `string` |
| Extensions | [CustomExtensionRepository](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.CustomExtensionRepository) | `custom_extension_repository` | `string` |
| Query | [DefaultOrder](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.DefaultOrder) | `default_order` | [DuckDBSortOrder](xref:DuckDB.NET.Data.DuckDBSortOrder) |
| Query | [DefaultNullOrder](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.DefaultNullOrder) | `default_null_order` | [DuckDBNullOrder](xref:DuckDB.NET.Data.DuckDBNullOrder) |
| Query | [DefaultCollation](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.DefaultCollation) | `default_collation` | `string` |
| Query | [PreserveInsertionOrder](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.PreserveInsertionOrder) | `preserve_insertion_order` | `bool` |
| Client | [CustomUserAgent](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.CustomUserAgent) | `custom_user_agent` | `string` |
| Client | [ErrorsAsJson](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.ErrorsAsJson) | `errors_as_json` | `bool` |

Properties of type `int` and `bool` are nullable. [Threads](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.Threads) must be at least 1. Sizes such as [MemoryLimit](xref:DuckDB.NET.Data.DuckDBConnectionStringBuilder.MemoryLimit) are strings in DuckDB's format, for example `10GB`.

The builder stores both `Data Source` and `DataSource` as `DataSource`, so a connection string can't hold two different data sources.

## Example connection strings

| Connection String | Description |
| ----------- | ----------- |
| `DataSource = :memory:` | Connect to a new in-memory database |
| `DataSource = :memory:?cache=shared` | Connect to a shared, in-memory database |
| `DataSource = train_services.db` | Connect to `train_services.db` |
| `DataSource = train_services.db;ACCESS_MODE=READ_ONLY` | Connect to `train_services.db`, make connection read-only |
| `DataSource = :memory:;threads=8;ACCESS_MODE=READ_ONLY` | Connect to a new in-memory database, limit threads to 8, make connection read-only |
| `DataSource = train_services.db;ACCESS_MODE=READ_ONLY;memory_limit=10GB` | Connect to `train_services.db`, make connection read-only, limit RAM usage to 10GB |

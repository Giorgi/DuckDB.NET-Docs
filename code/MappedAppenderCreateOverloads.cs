// Simple table name
public DuckDBMappedAppender<T, TMap> CreateAppender<T, TMap>(string table)
    where TMap : DuckDBAppenderMap<T>, new()

// Schema and table
public DuckDBMappedAppender<T, TMap> CreateAppender<T, TMap>(string? schema, string table)
    where TMap : DuckDBAppenderMap<T>, new()

// Catalog, schema, and table
public DuckDBMappedAppender<T, TMap> CreateAppender<T, TMap>(string? catalog, string? schema, string table)
    where TMap : DuckDBAppenderMap<T>, new()
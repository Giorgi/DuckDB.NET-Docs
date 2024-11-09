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

var data = connection.Query<(string, string, int, string, string)>("SELECT * FROM github_search('duckdb', 400);");

```

In the example above we define a table function, `github_search` that takes two parameters: a `string` parameter that is used as a search term and an `int` that filters results by stargazers. The first callback, `resultCallback`, provides access to the parameters that the functions was invoked with and returns a `TableFunction` instance that describes the columns that our table function has and an `IEnumerable<>` that serves as a result set of the invocation. The second callback, `mapperCallback` will be invoked by DuckDB.NET for every item in the result that. In this callback you should use the provided `IDuckDBDataWriter` array to write the item back to DuckDB.

Executing the above query produces the following result:
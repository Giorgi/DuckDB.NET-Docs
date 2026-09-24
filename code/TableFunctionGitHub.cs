using DuckDB.NET.Data;
using Octokit;

namespace Samples.Snippets;

public static class TableFunctionGitHub
{
    public static void Run(DuckDBConnection connection)
    {
        #region Example
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
            var repo = (Repository)item!;
            writers[0].WriteValue(repo.Name, rowIndex);
            writers[1].WriteValue(repo.Description, rowIndex);
            writers[2].WriteValue(repo.StargazersCount, rowIndex);
            writers[3].WriteValue(repo.Url, rowIndex);
            writers[4].WriteValue(repo.Owner.Login, rowIndex);
        });
        #endregion
    }
}

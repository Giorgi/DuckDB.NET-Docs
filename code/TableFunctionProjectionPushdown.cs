connection.RegisterTableFunction("employees",
    (IReadOnlyList<ProjectedColumn> projected, int count) =>
    {
        // projected contains only the columns DuckDB actually needs
        return FetchEmployees(count, projected.Select(p => p.Name));
    },
    e => new { e.Id, e.Name, e.Salary });

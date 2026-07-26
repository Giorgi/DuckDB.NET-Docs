connection.RegisterTableFunction("employees",
    (int count, [Named("max_rows")] int? limit) => GetEmployees(limit ?? count),
    e => new { e.Id, e.Name });

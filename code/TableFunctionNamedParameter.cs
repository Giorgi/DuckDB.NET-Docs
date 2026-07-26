connection.RegisterTableFunction("employees",
    (int count, [Named] string? prefix) =>
        GetEmployees(count).Select(e => e with { Name = (prefix ?? "") + e.Name }),
    e => new { e.Id, e.Name });

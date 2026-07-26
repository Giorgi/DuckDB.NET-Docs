connection.RegisterTableFunction("employees",
    (int count) => GetEmployees(count),
    e => new { e.Id, e.Name });

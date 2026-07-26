connection.RegisterTableFunction("employees",
    (int count, [Named] string? prefix, [Named] double? multiplier) =>
        GetEmployees(count).Select(e => e with
        {
            Name = (prefix ?? "") + e.Name,
            Salary = e.Salary * (multiplier ?? 1)
        }),
    e => new { e.Id, e.Name, e.Salary });

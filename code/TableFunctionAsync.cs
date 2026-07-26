connection.RegisterTableFunction("ext_async",
    (int count) => FetchEmployeesAsync(count).ToBlockingEnumerable(),
    e => new { e.Id, e.Name });

connection.RegisterScalarFunction<string, string>("echo_nullable", (readers, writer, rowCount) =>
{
    for (ulong i = 0; i < rowCount; i++)
    {
        var value = readers[0].GetValue<string>(i);
        writer.WriteValue(value ?? "was_null", i);
    }
}, new() { HandlesNulls = true });

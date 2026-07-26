connection.RegisterScalarFunction<long, long>("my_rand", (readers, writer, rowCount) =>
{
    for (ulong index = 0; index < rowCount; index++)
    {
        var value = 0L;

        if (readers.Count == 0)
        {
            value = Random.Shared.NextInt64();
        }

        if (readers.Count == 1)
        {
            value = Random.Shared.NextInt64(readers[0].GetValue<long>(index));
        }

        if (readers.Count == 2)
        {
            value = Random.Shared.NextInt64(readers[0].GetValue<long>(index), readers[1].GetValue<long>(index));
        }

        writer.WriteValue(value, index);
    }
}, new ScalarFunctionOptions { IsPureFunction = false }, @params: true);

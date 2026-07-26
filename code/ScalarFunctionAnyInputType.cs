connection.RegisterScalarFunction<object, string, string>("to_string", (readers, writer, rowCount) =>
{
    for (ulong index = 0; index < rowCount; index++)
    {
        var format = readers[1].GetValue<string>(index);

        var value = readers[0].GetValue(index);

        if (value is IFormattable formattable)
        {
            writer.WriteValue(formattable.ToString(format, CultureInfo.InvariantCulture), index);
        }
    }
});

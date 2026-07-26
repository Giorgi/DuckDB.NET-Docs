connection.RegisterScalarFunction("format_net",
    (object value, string format) => value is IFormattable f
        ? f.ToString(format, CultureInfo.InvariantCulture)
        : value.ToString());

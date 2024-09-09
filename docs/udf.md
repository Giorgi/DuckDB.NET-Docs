# User-Defined Functions in DuckDB.NET

DuckDB supports creating user-defined functions for expending the functionality of DuckDB. Starting from DuckDB.NET 1.1,
you can create scalar UDFs with C#.

## Scalar User-Defined Functions

To register scalar UDFs, call one of the [RegisterScalarFunction](xref:DuckDB.NET.Data.DuckDBConnection.RegisterScalarFunction``1(System.String,System.Action{DuckDB.NET.Data.Reader.IDuckDBDataReader[],DuckDB.NET.Data.Writer.IDuckDBDataWriter,System.UInt64},System.Boolean)) overloads specifying the function name, input parameter type(s), return type, and the actual callback for the scalar function:

```cs
connection.RegisterScalarFunction<int, bool>("is_prime", (readers, writer, rowCount) =>
{
    for (ulong index = 0; index < rowCount; index++)
    {
        var prime = true;
        var value = readers[0].GetValue<int>(index);

        for (int i = 2; i <= Math.Sqrt(value); i++)
        {
            if (value % i == 0)
            {
                prime = false;
                break;
            }
        }

        writer.WriteValue(prime, index);
    }
});

var primes = connection.Query<int>("SELECT i FROM range(2, 100) t(i) where is_prime(i::INT)").ToList();

//primes will be 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97
```

In the example above, `RegisterScalarFunction` has two type parameters: `int` indicates the input parameter type and `bool` indicates the return type of the function. The `readers` parameter passed to the callback is an array of type [`IDuckDBDataReader`](xref:DuckDB.NET.Data.Reader.IDuckDBDataReader) and will have a length that matches the number of input parameters. The `writer` parameter of type [`IDuckDBDataWriter`](xref:DuckDB.NET.Data.Writer.IDuckDBDataWriter) is used for writing to the output.

### Pure Scalar Functions

By default scalar UDFs are treated as [pure functions](https://en.wikipedia.org/wiki/Pure_function), meaning that the scalar function will return the same result when the input is the same. For example, if your table has repeated values, the `is_prime` will be called only once for every unique value. If your function doesn't follow this rule, you need to pass `false` to `isPureFunction` parameter:

```cs
connection.RegisterScalarFunction<long, long>("my_random_scalar", (readers, writer, rowCount) =>
{
    for (ulong index = 0; index < rowCount; index++)
    {
        var value = Random.Shared.NextInt64(readers[0].GetValue<long>(index));

        writer.WriteValue(value, index);
    }
}, false);

connection.Query<long>("SELECT my_random_scalar(i) FROM some_table");
```

### Variable Number of Arguments

Similar to the `params` keyword in C#, DuckDB scalar functions can also accept variable number of arguments. To register such function pass `true` to `params` parameter:

```cs
connection.RegisterScalarFunction<long, long>("my_rand", (readers, writer, rowCount) =>
{
    for (ulong index = 0; index < rowCount; index++)
    {
        var value = 0L;

        if (readers.Length == 0)
        {
            value = Random.Shared.NextInt64();
        }

        if (readers.Length == 1)
        {
            value = Random.Shared.NextInt64(readers[0].GetValue<long>(index));
        }

        if (readers.Length == 2)
        {
            value = Random.Shared.NextInt64(readers[0].GetValue<long>(index), readers[1].GetValue<long>(index));
        }

        writer.WriteValue(value, index);
    }
}, false, true);
```

In this example `my_rand` will return a random number if no parameter is passed to it, or return a random number less than the specified value, or return a random number between the two specified values.

```sql
SELECT my_rand() FROM some_table;

SELECT my_rand(upper_bound) FROM some_table;

SELECT my_rand(lower_bound, upper_bound) FROM some_table;
```

### Supporting Different Input Types

If your scalar function supports different input types, you can use `object` as input type. The following scalar function `to_string` formats the input according to the specified format and returns it to DuckDB:

```cs
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
```

You can call this function like this:

```sql
SELECT id, to_string(id, 'G'), order_date, to_string(order_date, 'dd-MM-yyyy'), amount, to_string(amount, 'G'), FROM TestTableAnyType
```

You can also inspect the type of the input and read the input in strongly-typed way:

```cs
//to_string accepts and object and format string and returns string
connection.RegisterScalarFunction<object, string, string>("to_string", (readers, writer, rowCount) =>
{
    for (ulong index = 0; index < rowCount; index++)
    {
        var format = readers[1].GetValue<string>(index);

        switch (readers[0].DuckDBType)
        {
            case DuckDBType.Integer:
                writer.WriteValue(readers[0].GetValue<int>(index).ToString(format, CultureInfo.InvariantCulture), index);
                break;
            case DuckDBType.Date:
                writer.WriteValue(readers[0].GetValue<DateOnly>(index).ToString(format, CultureInfo.InvariantCulture), index);
                break;
            case DuckDBType.Double:
                writer.WriteValue(readers[0].GetValue<double>(index).ToString(format, CultureInfo.InvariantCulture), index);
                break;
            default:
                writer.WriteValue(readers[0].GetValue(index).ToString(), index);
                break;
        }
    }
});
```
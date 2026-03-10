# Scalar User-Defined Functions

## High-Level API

> [!TIP]
> Introduced in DuckDB.NET 1.5.0, this is the recommended way to register scalar functions for most use cases.

Register a scalar function with a plain `Func<>` delegate. The framework handles row iteration, vector access, and null handling automatically.

```cs
connection.RegisterScalarFunction<int, bool>("is_prime", IsPrime);

var primes = connection.Query<int>("SELECT i FROM range(2, 100) t(i) WHERE is_prime(i::INT)").ToList();

//primes will be 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97
```

The API supports zero to four parameters:

```cs
// Zero parameters
connection.RegisterScalarFunction("the_answer", () => 42);

// One parameter
connection.RegisterScalarFunction<int, bool>("is_prime", IsPrime);

// Two parameters
connection.RegisterScalarFunction<long, long, long>("add", (a, b) => a + b);

// Three parameters
connection.RegisterScalarFunction<int, int, int, int>("clamp",
    (value, min, max) => Math.Clamp(value, min, max));
```

### Variable Arguments

Variable argument functions receive all inputs as an array. DuckDB allows calling them with any number of arguments:

```cs
connection.RegisterScalarFunction("sum_all", (long[] args) => args.Sum());
```

```sql
SELECT sum_all(1, 2, 3);    -- 6
SELECT sum_all(10);          -- 10
SELECT sum_all();            -- 0
```

### Dynamic Type Support

Use `object` as an input type to map to DuckDB's `ANY` type. This lets you write functions that accept any column type:

```cs
connection.RegisterScalarFunction<object, string>("net_type_name",
    value => value.GetType().Name);
```

```sql
SELECT net_type_name(42);                -- 'Int32'
SELECT net_type_name(42::BIGINT);        -- 'Int64'
SELECT net_type_name('hello');           -- 'String'
SELECT net_type_name('2024-01-01'::DATE); -- 'DateOnly'
```

You can combine `object` with typed parameters:

```cs
connection.RegisterScalarFunction("format_net",
    (object value, string format) => value is IFormattable f
        ? f.ToString(format, CultureInfo.InvariantCulture)
        : value.ToString());
```

### NULL Handling

By default, DuckDB short-circuits NULLs: if any input is NULL, the result is NULL and your function is never called. To handle NULLs explicitly, use nullable parameter types (`int?`, `string?`). DuckDB.NET detects nullable parameters at registration time and configures the function automatically:

```cs
connection.RegisterScalarFunction<int?, string>("describe_val",
    x => x.HasValue ? x.Value.ToString() : "nothing");
```

```sql
SELECT describe_val(42);        -- '42'
SELECT describe_val(NULL::INT); -- 'nothing'
```

This works for reference types too — `string?` (with nullable annotations enabled) opts into null handling, while `string` keeps default NULL propagation:

```cs
connection.RegisterScalarFunction<string?, string>("echo_or_default",
    s => s ?? "was_null");
```

You can mix nullable and non-nullable parameters. If any parameter is nullable, the function receives NULLs — but non-nullable parameters will throw a clear error if they get one:

```cs
connection.RegisterScalarFunction<int?, int, string>("coalesce_add",
    (a, b) => a.HasValue ? (a.Value + b).ToString() : b.ToString());
```

```sql
SELECT coalesce_add(NULL::INT, 5); -- '5'
SELECT coalesce_add(10, 5);       -- '15'
```

## Low-Level API

The low-level API gives you direct access to input vectors and the output vector for maximum control. You manually iterate over rows, read values by index, and write results:

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

`RegisterScalarFunction` has two type parameters: `int` indicates the input parameter type and `bool` indicates the return type of the function. The `readers` parameter passed to the callback is an array of type [`IDuckDBDataReader`](xref:DuckDB.NET.Data.DataChunk.Reader.IDuckDBDataReader) and will have a length that matches the number of input parameters. The `writer` parameter of type [`IDuckDBDataWriter`](xref:DuckDB.NET.Data.DataChunk.Writer.IDuckDBDataWriter) is used for writing to the output.

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

### NULL Handling

To handle NULLs with the low-level API, pass `HandlesNulls = true` via `ScalarFunctionOptions`. In scalar function callbacks, `GetValue<T>` returns `null` for NULL rows when `T` is a nullable type (`int?`, `string`, etc.):

```cs
connection.RegisterScalarFunction<string, string>("echo_nullable", (readers, writer, rowCount) =>
{
    for (ulong i = 0; i < rowCount; i++)
    {
        var value = readers[0].GetValue<string>(i);
        writer.WriteValue(value ?? "was_null", i);
    }
}, new() { HandlesNulls = true });
```

You can also check `readers[0].IsValid(index)` directly to test for NULL without reading the value.

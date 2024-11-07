# Working with composite types

DuckDB.NET supports reading all composite types such as `Array`, `Struct`, `List`, and `Map` except the `Union` composite type. The `Enum` type is supported as well. Writing composite types is partially supported. You can use [Parameterized statements](basic-usage.md#parameterized-statements) to write `LIST` or `ARRAY` types. Additionally, you can use [`DuckDBAppender`](xref:DuckDB.NET.Data.DuckDBAppender) to append to an `Enum`, `LIST` or `ARRAY` column.


## Reading Enum and Composite types

You can read an Enum or composite type by calling the [`GetValue`](xref:DuckDB.NET.Data.DuckDBDataReader.GetValue(System.Int32)) or the [`GetFieldValue<T>`](xref:DuckDB.NET.Data.DuckDBDataReader.GetFieldValue``1(System.Int32)) method.

The following table lists the mapping that determines what type will be returned when using the `GetValue` method to read enum and composite types:

| DuckDB Type      | .NET Type| Comment |
|-----------------|----------------------|---|
| Enum        | string | The string representation of the DuckDB Enum member|
| Array | List<`T`> | Type of T will be the underlying type of the Array column|
| List        | List<`T`> | Type of T will be the underlying type of the List column|
| Struct | Dictionary<string, object> | The keys of the dictionary will be keys of the corresponding DuckDB Struct keys. |
| Map        | Dictionary<`TKey, TValue`> | The keys of the dictionary will be keys of the corresponding DuckDB Map keys. Type of `TKey` will be the underlying type of the Map keys.|

When using the `GetFieldValue<T>` method, you can use the following mapping to read enum and composite types:

| DuckDB Type      | .NET Type| Comment |
|-----------------|----------------------|---|
| Enum        | T | T must be an Enum type. The Enum members are not required to match the Enum type in the database as only the underlying numeric value is used for mapping.|
| Array | List`<T>` | The underlying type of the Array should be readable as `T`|
| List        | List`<T>` | The underlying type of the List should be readable as `T`|
| Struct | T | T must have a parameterless constructor. Struct keys that have no corresponding property on type `T` will be ignored.  |
| Map        | Dictionary`<TKey, TValue>` | The underlying type of the Map key should be readable as `TKey`. The underlying type of the Map value should be readable as `TValue`|

## Nested composite types

Reading arbitrary nested composite types is fully supported by DuckDB.NET. DuckDB.NET supports Lists of Lists, Lists of Structs, Lists of Lists of Lists, Structs with Lists properties, Maps with Struct values, and so on can be read by using the `GetFieldValue<T>` method and passing corresponding type `T` as a parameter.

## Nested types and nullability

When reading a List/Array of non-nullable types (int, short, long, bool, etc) an exception will be thrown if the List contains `null` and you are trying to read it as `List<int>` (or other non-nullable type). To read a List with nullable values pass the corresponding nullable type to the `GetFieldValue<T>` method.  For example, to read a list of nullable ints use `DuckDBDataReader.GetFieldValue<List<int?>>`.

When reading a Struct, an exception will be thrown if the struct entry contains a null value and the corresponding property is non-nullable.

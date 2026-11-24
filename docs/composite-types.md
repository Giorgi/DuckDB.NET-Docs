---
description: "Read and write DuckDB composite types - LIST, ARRAY, STRUCT, MAP, and ENUM - from .NET, including nested types and nullability."
---

# Working with composite types

DuckDB.NET supports reading all composite types such as `Array`, `Struct`, `List`, and `Map` except the `Union` composite type. The `Enum` type is supported as well. Writing composite types is partially supported. You can use [Parameterized statements](basic-usage.md#parameterized-statements) to write `LIST` or `ARRAY` types. Additionally, you can use [`DuckDBAppender`](xref:DuckDB.NET.Data.DuckDBAppender) to append to an `Enum`, `LIST` or `ARRAY` column, including nested types. Starting from version 1.2.0, DuckDB.NET supports writing nested composite types with prepared statements. You can insert/update Lists of Lists, Lists of Lists of Lists, Arrays of Arrays, Lists of Arrays, Arrays of Lists, or any combination of these types of arbitrary depth.


## Reading Enum and Composite types

You can read an Enum or composite type by calling the [`GetValue`](xref:DuckDB.NET.Data.DuckDBDataReader.GetValue(System.Int32)) or the [`GetFieldValue<T>`](xref:DuckDB.NET.Data.DuckDBDataReader.GetFieldValue``1(System.Int32)) method.

The following table lists the mapping that determines what type will be returned when using the `GetValue` method to read enum and composite types:

| DuckDB Type      | .NET Type| Comment |
|-----------------|----------------------|---|
| Enum        | `string` | The string representation of the DuckDB Enum member|
| Array | `List<T>` | Type of `T` will be the underlying type of the Array column|
| List        | `List<T>` | Type of `T` will be the underlying type of the List column|
| Struct | `Dictionary<string, object>` | The keys of the dictionary will be keys of the corresponding DuckDB Struct keys. |
| Map        | `Dictionary<TKey, TValue>` | The keys of the dictionary will be keys of the corresponding DuckDB Map keys. Type of `TKey` will be the underlying type of the Map keys.|

When using the `GetFieldValue<T>` method, you can use the following mapping to read enum and composite types:

| DuckDB Type      | .NET Type| Comment |
|-----------------|----------------------|---|
| Enum        | `T` | `T` can be a C# enum type, an integer type or `string`. A C# enum or an integer type gets the position of the value in the list of names of the DuckDB Enum, counted from zero. So the names of the C# enum members don't have to match the names in the database. `string` gets the name.|
| Array | `List<T>` | The underlying type of the Array should be readable as `T`. A class derived from `List<T>` or another collection that implements `IList`, such as `Collection<T>`, can be used too.|
| List        | `List<T>` | The underlying type of the List should be readable as `T`. A class derived from `List<T>` or another collection that implements `IList`, such as `Collection<T>`, can be used too.|
| Struct | `T` | `T` can be a class with a parameterless constructor, a C# struct or `Dictionary<string, object>`. Struct keys are matched to properties by name, ignoring case. Struct keys that have no corresponding property on type `T` will be ignored, and properties with no corresponding key keep their value. |
| Map        | `Dictionary<TKey, TValue>` | The underlying type of the Map key should be readable as `TKey`. The underlying type of the Map value should be readable as `TValue`. A class derived from `Dictionary<TKey, TValue>` or another `IDictionary`, such as `SortedDictionary<TKey, TValue>`, can be used too.|

A class derived from `List<T>` or `Dictionary<TKey, TValue>` is created as that class and filled with the item, key and value types of its base class, so the derived class doesn't have to be generic itself.

List and Array items, Map keys and values, and Struct fields are converted the same way as a column of their type, as described in [Type mappings](type-mapping.md). For example, an `INTEGER[]` column can be read as `List<long>` and a `BLOB[]` column as `List<byte[]>`. A `HUGEINT` Struct field can be read into a `long` property if the value fits. A value that can't be converted throws as it would in a column, in most cases with `InvalidCastException`.

The target type must be able to hold the value, or `GetFieldValue<T>` throws `InvalidCastException`. A List or Array can't be read into an interface or an abstract class such as `IEnumerable<int>` or `IList<int>`, or into a type that doesn't implement `IList`, such as `HashSet<int>`. A Map can't be read into an interface or an abstract class such as `IDictionary<string, int>`, or into a type that doesn't implement `IDictionary`, such as `string` or `List<KeyValuePair<string, int>>`. A list of `object` and a dictionary with `object` keys or values are refused too, and so is a class derived from `Dictionary<string, object>` when reading a Map. Use `GetValue` to read such a column with its default types.

## Nested composite types

Reading arbitrary nested composite types is fully supported by DuckDB.NET. Lists of Lists, Lists of Structs, Lists of Lists of Lists, Structs with Lists properties, Maps with Struct values, and so on can be read by using the `GetFieldValue<T>` method and passing corresponding type `T` as a parameter.

## Nested types and nullability

When reading a List/Array of non-nullable types (int, short, long, bool, etc) an `InvalidCastException` will be thrown if the List contains `null` and you are trying to read it as `List<int>` (or other non-nullable type). `GetValue` throws too, because it reads an `INTEGER[]` column as `List<int>`. To read a List with nullable values pass the corresponding nullable type to the `GetFieldValue<T>` method. For example, to read a list of nullable ints use `DuckDBDataReader.GetFieldValue<List<int?>>`.

A Map with a `null` value works the same way. `MAP {1: 10, 2: NULL}` can be read as `Dictionary<int, int?>`, while `Dictionary<int, int>` and `GetValue` throw `InvalidCastException`.

When reading a Struct, an `InvalidCastException` will be thrown if the struct entry contains a null value and the corresponding property is non-nullable. If the property is nullable, a null value leaves it as it is, so it keeps its initial value.

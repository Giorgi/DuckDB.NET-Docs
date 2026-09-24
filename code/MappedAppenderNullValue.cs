using DuckDB.NET.Data.Mapping;

namespace Samples.Snippets.MappedAppenderNullValueSnippet;

#region Example
public class PersonMap : DuckDBAppenderMap<Person>
{
    public PersonMap()
    {
        Map(p => p.Id);
        Map(p => p.Name);
        NullValue();  // Inserts NULL for column 2
    }
}
#endregion

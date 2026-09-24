using DuckDB.NET.Data.Mapping;

namespace Samples.Snippets.MappedAppenderDefaultValueSnippet;

#region Example
public class PersonMap : DuckDBAppenderMap<Person>
{
    public PersonMap()
    {
        Map(p => p.Id);
        Map(p => p.Name);
        DefaultValue();  // Uses table's default for column 2
    }
}
#endregion

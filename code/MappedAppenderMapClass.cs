public class PersonMap : DuckDBAppenderMap<Person>
{
    public PersonMap()
    {
        Map(p => p.Id);        // Maps to column 0
        Map(p => p.Name);      // Maps to column 1
        Map(p => p.Height);    // Maps to column 2
        Map(p => p.BirthDate); // Maps to column 3
    }
}
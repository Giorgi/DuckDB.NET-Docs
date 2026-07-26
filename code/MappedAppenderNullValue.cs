public class PersonMap : DuckDBAppenderMap<Person>
{
    public PersonMap()
    {
        Map(p => p.Id);
        Map(p => p.Name);
        NullValue();  // Inserts NULL for column 2
    }
}
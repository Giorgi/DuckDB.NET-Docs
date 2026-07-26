// Table schema: CREATE TABLE test(id INTEGER, value REAL, date TIMESTAMP);

public class BadMap : DuckDBAppenderMap<MyData>
{
    public BadMap()
    {
        Map(d => d.Id);        // int -> INTEGER ✓
        Map(d => d.Date);      // DateTime -> REAL ✗ TYPE MISMATCH!
        Map(d => d.Value);     // float -> TIMESTAMP ✗ TYPE MISMATCH!
    }
}

// This will throw InvalidOperationException when creating the appender
var appender = connection.CreateAppender<MyData, BadMap>("test");
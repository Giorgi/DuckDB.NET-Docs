using var connection = new DuckDBConnection("DataSource=:memory:");
connection.Open();

using (var duckDbCommand = connection.CreateCommand())
{
  var table = "CREATE TABLE AppenderTest(foo INTEGER, bar INTEGER);";
  duckDbCommand.CommandText = table;
  duckDbCommand.ExecuteNonQuery();
}

var rows = 100;
using (var appender = connection.CreateAppender("AppenderTest"))
{
  for (var i = 0; i < rows; i++)
  {
    var row = appender.CreateRow();
    row.AppendValue(i).AppendValue(i+2).EndRow();
  }
}
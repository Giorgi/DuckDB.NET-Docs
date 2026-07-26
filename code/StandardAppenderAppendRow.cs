using (var appender = connection.CreateAppender("AppenderTest"))
{
    for (var i = 0; i < rows; i++)
    {
        appender.AppendRow(i, static (row, value) =>
        {
            row.AppendValue(value).AppendValue(value + 2);
        });
    }
}

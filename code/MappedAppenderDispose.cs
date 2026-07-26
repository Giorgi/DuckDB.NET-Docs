using (var appender = connection.CreateAppender<Person, PersonMap>("people"))
{
    appender.AppendRecords(records);
} // Automatically disposed here
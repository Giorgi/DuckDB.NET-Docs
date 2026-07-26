var people = new[]
{
    new Person { Id = 1, Name = "Alice" },
    new Person { Id = 2, Name = "Bob" }
};

using var appender = connection.CreateAppender<Person, PersonMap>("people");
appender.AppendRecords(people);
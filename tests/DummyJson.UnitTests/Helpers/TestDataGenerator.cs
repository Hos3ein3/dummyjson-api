using AutoFixture;
using Bogus;
using DummyJson.Domain.Todos;

namespace DummyJson.UnitTests.Helpers;

public static class TestDataGenerator
{
    /// <summary>
    /// Generates fake Todos using Bogus for seeding databases or mocking.
    /// </summary>
    public static List<Todo> GenerateFakeTodos(int count)
    {
        var todoFaker = new Faker<Todo>()
            .CustomInstantiator(f =>
            {
                var userId = f.Random.Guid();
                var text = f.Lorem.Sentence();
                return Todo.Create(userId, text).Value;
            });

        return todoFaker.Generate(count);
    }

    /// <summary>
    /// Demonstrates using AutoFixture to create a single anonymous string.
    /// </summary>
    public static string GenerateAnonymousTitle()
    {
        var fixture = new Fixture();
        return fixture.Create<string>();
    }
}

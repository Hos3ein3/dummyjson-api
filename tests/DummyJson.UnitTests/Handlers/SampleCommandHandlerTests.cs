using AutoFixture;
using DummyJson.Application.Common.UnitOfWork;
using DummyJson.Application.Samples.Commands;
using DummyJson.Domain.Todos;
using FluentAssertions;
using Moq;
using Shouldly;
using DummyJson.UnitTests.Helpers;
using DummyJson.Application.Common.Repository;

namespace DummyJson.UnitTests.Handlers;

public class SampleCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NormalCommandHandler _handler;
    private readonly Fixture _fixture;

    public SampleCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new NormalCommandHandler(_unitOfWorkMock.Object);
        _fixture = new Fixture();

        // Setup mock repository for Todo
        var todoRepoMock = new Mock<IRepository<Todo, Guid>>();
        _unitOfWorkMock.Setup(u => u.Repository<Todo, Guid>()).Returns(todoRepoMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldReturnSuccessResult_UsingFluentAssertions()
    {
        // Arrange
        // Using AutoFixture to generate a random string title
        var command = new NormalCommand(_fixture.Create<string>());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert (FluentAssertions)
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldReturnSuccessResult_UsingShouldly()
    {
        // Arrange
        // Using our Bogus test data generator just for an example string
        var command = new NormalCommand(TestDataGenerator.GenerateAnonymousTitle());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert (Shouldly)
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        
        _unitOfWorkMock.Verify(u => u.Repository<Todo, Guid>().AddAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTitleIsEmpty_ShouldReturnFailure_UsingFluentAssertions()
    {
        // Arrange
        var command = new NormalCommand(string.Empty);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("todo"); // It throws with nameof(todo) as Code in Todo.Create
        
        // Ensure no database saves happened
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Bogus_TestDataGenerator_ShouldGenerateValidTodos_UsingShouldly()
    {
        // Arrange
        int count = 5;

        // Act
        var todos = TestDataGenerator.GenerateFakeTodos(count);

        // Assert (Shouldly)
        todos.Count.ShouldBe(count);
        todos.ShouldAllBe(t => t.Id != Guid.Empty);
        todos.ShouldAllBe(t => !string.IsNullOrEmpty(t.TodoText));
    }
}

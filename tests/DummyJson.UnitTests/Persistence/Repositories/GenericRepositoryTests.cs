using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DummyJson.Domain.Todos;
using DummyJson.Persistence.Context;
using DummyJson.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DummyJson.UnitTests.Persistence.Repositories;

public class GenericRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GenericRepository<Todo, Guid> _sut;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new GenericRepository<Todo, Guid>(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var todo = Todo.Create(Guid.NewGuid(), "Test Todo").Value;

        // Act
        await _sut.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Todos.FindAsync(todo.Id);
        result.Should().NotBeNull();
        result!.TodoText.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        var todo = Todo.Create(Guid.NewGuid(), "Test Todo").Value;
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(todo.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(todo.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var todo1 = Todo.Create(Guid.NewGuid(), "Todo 1").Value;
        var todo2 = Todo.Create(Guid.NewGuid(), "Todo 2").Value;
        _context.Todos.AddRange(todo1, todo2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Id == todo1.Id);
        result.Should().Contain(t => t.Id == todo2.Id);
    }

    [Fact]
    public async Task Update_ShouldModifyEntity()
    {
        // Arrange
        var todo = Todo.Create(Guid.NewGuid(), "Old Todo").Value;
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        todo.Complete(); // Completes the todo
        _sut.Update(todo);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Todos.FindAsync(todo.Id);
        result!.Completed.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldPerformSoftDelete_WhenEntitySupportsISoftDelete()
    {
        // Arrange
        var todo = Todo.Create(Guid.NewGuid(), "Test Todo").Value;
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        _sut.Delete(todo);
        await _context.SaveChangesAsync();

        // Assert
        // In EF Core, if global query filter is applied, FindAsync might still return it 
        // if we ignore query filters. But via generic repo it uses DbSet.Remove or Update.
        // The GenericRepository checks if it's ISoftDelete and calls Delete(), then Update().
        var entity = await _context.Todos.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == todo.Id);
        entity.Should().NotBeNull();
        entity!.IsDeleted.Should().BeTrue();
    }
}

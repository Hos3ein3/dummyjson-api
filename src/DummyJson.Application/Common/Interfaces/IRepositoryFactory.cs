using DummyJson.Application.Common.Repository;

namespace DummyJson.Application.Common.Interfaces;

/// <summary>
/// Factory that provides strongly-typed, domain-specific repositories
/// without exposing the generic <c>IUnitOfWork.Repository&lt;T,TId&gt;()</c> method.
///
/// Use this when you need a named repository with custom query methods
/// (e.g. <see cref="IUserRepository.GetByEmailAsync"/>).
/// For simple CRUD you can still use <c>IUnitOfWork.Repository&lt;T,TId&gt;()</c>.
/// </summary>
public interface IRepositoryFactory
{
    /// <summary>Gets the <see cref="IUserRepository"/>.</summary>
    IUserRepository Users { get; }

    /// <summary>Gets the <see cref="ICartRepository"/>.</summary>
    ICartRepository Carts { get; }

    /// <summary>Gets the <see cref="ITodoRepository"/>.</summary>
    ITodoRepository Todos { get; }

    /// <summary>Gets the <see cref="IQuoteRepository"/>.</summary>
    IQuoteRepository Quotes { get; }

    /// <summary>Gets the <see cref="ICommentRepository"/>.</summary>
    ICommentRepository Comments { get; }

    /// <summary>Gets the <see cref="IRecipeRepository"/>.</summary>
}

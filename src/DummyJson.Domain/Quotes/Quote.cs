using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Quotes;

/// <summary>
/// Quote aggregate root — corresponds to DummyJSON /quotes resource.
/// Stored in MongoDB via MongoDbContext.
/// </summary>
public sealed class Quote : MongoEntity, IAuditable, ISoftDelete
{
    private Quote() { }

    private Quote(string quote, string author)
    {
        QuoteText = quote;
        Author = author;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string QuoteText { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;

    // IAuditable
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<Quote> Create(string quote, string author)
    {
        if (string.IsNullOrWhiteSpace(quote))
            return Result.Failure<Quote>(Error.Validation(nameof(quote), "Quote text cannot be empty."));

        if (string.IsNullOrWhiteSpace(author))
            return Result.Failure<Quote>(Error.Validation(nameof(author), "Author cannot be empty."));

        return Result.Success(new Quote(quote, author));
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public Result Update(string quote, string author)
    {
        if (string.IsNullOrWhiteSpace(quote))
            return Result.Failure(Error.Validation(nameof(quote), "Quote text cannot be empty."));

        QuoteText = quote;
        Author = author;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Carts;

/// <summary>
/// Cart aggregate root — a user's shopping cart.
/// </summary>
public sealed class Cart : AggregateRoot<Guid>, IAuditable, ISoftDelete
{
    private readonly List<CartItem> _items = [];
    private Cart() { }

    private Cart(Guid id, Guid userId) : base(id)
    {
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(i => i.Total);
    public decimal DiscountedTotal => _items.Sum(i => i.DiscountedTotal);
    public int TotalProducts => _items.Count;
    public int TotalQuantity => _items.Sum(i => i.Quantity);

    // IAuditable
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public static Cart Create(Guid userId) => new(Guid.CreateVersion7(), userId);

    public Result AddItem(Guid productId, string title, decimal price, decimal discountPercentage, int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(Error.Validation(nameof(quantity), "Quantity must be positive."));

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(Guid.CreateVersion7(), productId, title, price, discountPercentage, quantity));
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return Result.Failure(CommonErrors.NotFound(nameof(CartItem), productId));

        _items.Remove(item);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}

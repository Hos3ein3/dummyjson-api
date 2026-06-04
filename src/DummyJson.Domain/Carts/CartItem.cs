using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Carts;

/// <summary>
/// Cart item entity owned by the Cart aggregate.
/// </summary>
public sealed class CartItem : Entity<Guid>
{
    private CartItem() { }

    internal CartItem(Guid id, Guid productId, string title, decimal price, decimal discountPercentage, int quantity)
        : base(id)
    {
        ProductId = productId;
        Title = title;
        Price = price;
        DiscountPercentage = discountPercentage;
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public int Quantity { get; private set; }

    public decimal Total => Price * Quantity;
    public decimal DiscountedTotal => Total * (1 - DiscountPercentage / 100);

    internal void IncreaseQuantity(int amount) => Quantity += amount;
    internal void SetQuantity(int quantity) => Quantity = quantity;
}

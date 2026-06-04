using DummyJson.Application.Common.CQRS;
using DummyJson.Application.Common.UnitOfWork;
using DummyJson.Application.Products.Commands;
using SharedKernel.Results;
using DummyJson.Domain.Products;

namespace DummyJson.Application.Products.Handlers;

// ── Create ───────────────────────────────────────────────────────────────────

public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public CreateProductCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        
        var result = Product.Create(
            command.Title, command.Description, command.Price, command.DiscountPercentage,
            command.Stock, command.Brand, command.Category, command.Thumbnail,
            command.Images, command.Tags, command.Sku, command.Barcode,
            command.MinimumOrderQuantity, command.WarrantyInformation,
            command.ShippingInformation, command.AvailabilityStatus, command.ReturnPolicy);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var repo = _uow.Repository<Product, Guid>();
        await repo.AddAsync(result.Value, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Success<Guid>(result.Value.Id);
    }
}

// ── Update ───────────────────────────────────────────────────────────────────

public sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public UpdateProductCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var repo = _uow.Repository<Product, Guid>();
        var product = await repo.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
            return Result.Failure(CommonErrors.NotFound(nameof(Product), command.Id));

        var result = product.UpdateDetails(command.Title, command.Description, command.Price, command.Stock);
        if (result.IsFailure) return result;

        repo.Update(product);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// ── Delete ───────────────────────────────────────────────────────────────────

public sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public DeleteProductCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken = default)
    {
        var repo = _uow.Repository<Product, Guid>();
        var product = await repo.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
            return Result.Failure(CommonErrors.NotFound(nameof(Product), command.Id));

        product.Delete();
        repo.Update(product);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

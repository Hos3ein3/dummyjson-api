using DummyJson.Application.Common.CQRS;
using DummyJson.Application.Common.UnitOfWork;
using DummyJson.Application.Products.Commands;
using SharedKernel.Results;
using DummyJson.Domain.Products;
using DummyJson.Application.Common.Repository;

namespace DummyJson.Application.Products.Handlers;

// ── Create ───────────────────────────────────────────────────────────────────

public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly IProductImageRepository _imageRepo;

    public CreateProductCommandHandler(IUnitOfWork uow, IProductImageRepository imageRepo)
    {
        _uow = uow;
        _imageRepo = imageRepo;
    }

    public async Task<Result<Guid>> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
    {
        var result = Product.Create(
            command.Title, command.Description, command.Price, command.DiscountPercentage,
            command.Stock, command.Brand, Guid.Empty, command.Thumbnail,
            command.Sku, command.Barcode,
            command.MinimumOrderQuantity, command.WarrantyInformation,
            command.ShippingInformation, command.AvailabilityStatus, command.ReturnPolicy);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var repo = _uow.Repository<Product, Guid>();
        await repo.AddAsync(result.Value, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        // Save images to MongoDB
        if (command.Images is not null && command.Images.Any())
        {
            foreach (var imageUrl in command.Images)
            {
                var productImage = new ProductImage(result.Value.Id, imageUrl);
                await _imageRepo.InsertAsync(productImage, cancellationToken);
            }
        }

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

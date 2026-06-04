using DummyJson.Application.Auth.Commands;
using DummyJson.Application.Common.CQRS;
using DummyJson.Application.Common.Dispatcher;
using DummyJson.Application.Products.Commands;
using DummyJson.Application.Products.Handlers;
using SharedKernel.Results;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ── Dispatcher ────────────────────────────────────────────────────────
        services.AddScoped<IDispatcher, Dispatcher>();

        // ── Generic Service ───────────────────────────────────────────────────
        services.AddScoped(typeof(DummyJson.Application.Common.Interfaces.IGenericService<,>), typeof(DummyJson.Application.Common.Services.GenericService<,>));

        // ── Mapster ───────────────────────────────────────────────────────────
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // ── FluentValidation ──────────────────────────────────────────────────
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // ── Product Handlers ──────────────────────────────────────────────────
        services.AddScoped<ICommandHandler<CreateProductCommand, Result<Guid>>, CreateProductCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProductCommand, Result>, UpdateProductCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteProductCommand, Result>, DeleteProductCommandHandler>();

        // ── Domain Event Handlers (via Scrutor) ───────────────────────────────
        services.Scan(selector => selector
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(filter => filter.AssignableTo(typeof(IDomainEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // ── Integration Event Handlers (via Scrutor) ──────────────────────────
        services.Scan(selector => selector
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(filter => filter.AssignableTo(typeof(IIntegrationEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}

using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Users;
using DummyJson.Application.Auth.Commands;
using DummyJson.Infrastructure.Caching;
using DummyJson.Persistence.Context;
using DummyJson.API.Endpoints;
using ArchUnitNET.Fluent;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;

namespace DummyJson.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(AppDbContext).Assembly, // Persistence
            typeof(RedisCacheService).Assembly, // Infrastructure
            typeof(RegisterByEmailRequest).Assembly, // Application
            typeof(Entity<>).Assembly, // Domain
            typeof(AuthEndpoints).Assembly // API
        )
        .Build();

    private readonly IObjectProvider<Class> DomainLayer = Classes().That().ResideInAssembly(typeof(Entity<>).Assembly).As("Domain Layer");
    private readonly IObjectProvider<Class> ApplicationLayer = Classes().That().ResideInAssembly(typeof(RegisterByEmailRequest).Assembly).As("Application Layer");
    private readonly IObjectProvider<Class> InfrastructureLayer = Classes().That().ResideInAssembly(typeof(RedisCacheService).Assembly).As("Infrastructure Layer");
    private readonly IObjectProvider<Class> PersistenceLayer = Classes().That().ResideInAssembly(typeof(AppDbContext).Assembly).As("Persistence Layer");
    private readonly IObjectProvider<Class> ApiLayer = Classes().That().ResideInAssembly(typeof(AuthEndpoints).Assembly).As("API Layer");

    [Fact]
    public void DomainLayer_ShouldNotHaveDependenciesOnOtherLayers()
    {
        IArchRule rule = Classes().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(PersistenceLayer)
            .AndShould().NotDependOnAny(ApiLayer);

        rule.Check(Architecture);
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependenciesOnInfrastructureOrPersistenceOrApi()
    {
        IArchRule rule = Classes().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(PersistenceLayer)
            .AndShould().NotDependOnAny(ApiLayer);

        rule.Check(Architecture);
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotHaveDependenciesOnApiLayer()
    {
        IArchRule rule = Classes().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(ApiLayer);

        rule.Check(Architecture);
    }

    [Fact]
    public void PersistenceLayer_ShouldNotHaveDependenciesOnApiLayer()
    {
        IArchRule rule = Classes().That().Are(PersistenceLayer)
            .Should().NotDependOnAny(ApiLayer);

        rule.Check(Architecture);
    }

    [Fact]
    public void Endpoints_ShouldResideInApiLayer()
    {
        IArchRule rule = Classes().That().HaveNameEndingWith("Endpoints")
            .Should().ResideInAssembly(typeof(AuthEndpoints).Assembly);

        rule.Check(Architecture);
    }
}

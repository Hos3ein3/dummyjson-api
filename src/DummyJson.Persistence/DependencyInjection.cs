using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Common.Repository;
using DummyJson.Application.Common.UnitOfWork;
using DummyJson.Persistence.Context;
using DummyJson.Persistence.Repositories;
using DummyJson.Persistence.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UoW = DummyJson.Persistence.UnitOfWork.UnitOfWork;

namespace DummyJson.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var dbProvider = configuration["DatabaseProvider"] ?? "PostgreSQL";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("DummyJsonDb");
            }
            else if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("SqlServer"),
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            }
            else
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("PostgreSQL"),
                    npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            }
        });

        // ASP.NET Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddUserManager<DummyJson.Persistence.Identity.ApplicationUserManager>()
        .AddDefaultTokenProviders();

        // MongoDB
        services.AddSingleton<MongoDbContext>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UoW>();

        services.AddScoped(typeof(DummyJson.Application.Common.Interfaces.IMongoRepository<>), typeof(Repositories.MongoRepository<>));

        // ── Database & Repository Factories ───────────────────────────────────────
        services.AddScoped<IDatabaseFactory, DatabaseFactory>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();

        // ── Domain-specific repositories (Scoped, lazy-instantiated via RepositoryFactory) ─
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IRecipeRepository, RecipeRepository>();

        // ── Data Seeder ─────────────────────────────────────────────────────────── & Auth
        services.AddScoped<DataSeeder>();
        services.AddScoped<DummyJson.Application.Auth.Services.IRefreshTokenService, DummyJson.Persistence.Identity.RefreshTokenService>();

        return services;
    }
}

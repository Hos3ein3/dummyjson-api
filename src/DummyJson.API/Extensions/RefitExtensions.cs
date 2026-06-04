using DummyJson.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Refit;

namespace DummyJson.API.Extensions;

public static class RefitExtensions
{
    public static IServiceCollection AddCustomRefitClients(this IServiceCollection services)
    {
        // Register Refit clients
        services.AddRefitClient<ISampleRefitClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com"))
            .AddStandardResilienceHandler();

        return services;
    }
}

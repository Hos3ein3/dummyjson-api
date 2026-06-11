namespace DummyJson.API.Extensions;

public static class ScalarUiDocumentExtensions
{
    public static IServiceCollection AddDocumentsWithVersioning(this IServiceCollection services)
    {
services.AddOpenApi("v1", options =>
{
    
    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Info.Title = "DummyJson API";
        document.Info.Description = "Clean Architecture + DDD backend for DummyJSON data.";
        document.Info.Version = "v1";

        var httpContextAccessor = context.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        var request = httpContextAccessor.HttpContext?.Request;

        var scheme = request?.Scheme ?? "https";
        var host = request?.Host.Value ?? "dummyjson-api.behzadifard.me";

        document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
        {
            new() { Url = $"{scheme}://{host}" }
        };

        return Task.CompletedTask;
    });
    
});

services.AddOpenApi("v2", options =>
{
    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Info.Title = "DummyJson API";
        document.Info.Description = "Clean Architecture + DDD backend for DummyJSON data.";
        document.Info.Version = "v2";
        //document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer> { new Microsoft.OpenApi.Models.OpenApiServer { Url = "/" } };
        var httpContextAccessor = context.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        var request = httpContextAccessor.HttpContext?.Request;

        var scheme = request?.Scheme ?? "https";
        var host = request?.Host.Value ?? "dummyjson-api.behzadifard.me";

        document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
        {
            new() { Url = $"{scheme}://{host}" }
        };

        return Task.CompletedTask;
        return Task.CompletedTask;
    });
});
        
        return services;
    }
    
}
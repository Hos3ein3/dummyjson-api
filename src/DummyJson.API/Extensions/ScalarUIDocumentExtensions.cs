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
        var request = context.ApplicationServices
            .GetRequiredService<IHttpContextAccessor>()
            .HttpContext?.Request;

        if (request is not null)
        {
            document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
            {
                new()
                {
                    // Use X-Forwarded-Proto scheme (https) instead of internal http
                    Url = $"{request.Scheme}://{request.Host}"
                }
            };
        }
        //document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer> { new Microsoft.OpenApi.Models.OpenApiServer { Url = "/" } };
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
       
        var request = context.ApplicationServices
            .GetRequiredService<IHttpContextAccessor>()
            .HttpContext?.Request;

        if (request is not null)
        {
            document.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
            {
                new()
                {
                    // Use X-Forwarded-Proto scheme (https) instead of internal http
                    Url = $"{request.Scheme}://{request.Host}"
                }
            };
        }
        return Task.CompletedTask;
    });
});
        
        return services;
    }
    
}
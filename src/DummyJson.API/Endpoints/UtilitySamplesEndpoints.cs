using System;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using UnitsNet;
using System.IO;

namespace DummyJson.API.Endpoints;

public static class UtilitySamplesEndpoints
{
    public static void MapUtilitySamplesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/utilities").WithTags("Utilities");

        // ── Humanizer ─────────────────────────────────────────────────────────
        
        group.MapGet("/humanizer/timespan", () =>
        {
            var timeSpan = TimeSpan.FromDays(2).Add(TimeSpan.FromHours(4));
            return Results.Ok(new 
            { 
                Original = timeSpan.ToString(),
                Humanized = timeSpan.Humanize(2) // "2 days, 4 hours"
            });
        });

        group.MapGet("/humanizer/text", (string input) =>
        {
            // e.g., input="some_title_here" -> "Some title here"
            return Results.Ok(new 
            { 
                Original = input,
                Humanized = input.Humanize()
            });
        });

        // ── UnitsNet ──────────────────────────────────────────────────────────

        group.MapGet("/units/weight", (double kilograms) =>
        {
            var weight = Mass.FromKilograms(kilograms);
            return Results.Ok(new 
            { 
                Kilograms = weight.Kilograms,
                Pounds = weight.Pounds,
                Ounces = weight.Ounces,
                StringRepresentation = weight.ToUnit(UnitsNet.Units.MassUnit.Pound).ToString()
            });
        });

        group.MapGet("/units/temperature", (double celsius) =>
        {
            var temp = Temperature.FromDegreesCelsius(celsius);
            return Results.Ok(new 
            { 
                Celsius = temp.DegreesCelsius,
                Fahrenheit = temp.DegreesFahrenheit,
                Kelvin = temp.Kelvins
            });
        });

        // ── ImageSharp ────────────────────────────────────────────────────────

        group.MapPost("/images/resize", async (IFormFile file, int width, int height) =>
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file uploaded.");

            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);
            
            image.Mutate(x => x.Resize(width, height));

            var outStream = new MemoryStream();
            // Saving as JPEG for the sample
            await image.SaveAsJpegAsync(outStream);
            outStream.Position = 0;

            return Results.File(outStream, "image/jpeg", $"resized_{width}x{height}.jpg");
        }).DisableAntiforgery();
    }
}

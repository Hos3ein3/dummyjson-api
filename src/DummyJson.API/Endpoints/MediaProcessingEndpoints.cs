using System;
using System.Threading;
using DummyJson.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DummyJson.API.Endpoints;

public static class MediaProcessingEndpoints
{
    public static void MapMediaProcessingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/media").WithTags("Media Processing");

        // ── Image Processing ──────────────────────────────────────────────────

        group.MapPost("/image/shrink", async (IFormFile file, IFileService fileService, int quality = 75, CancellationToken ct = default) =>
        {
            using var stream = file.OpenReadStream();
            var result = await fileService.ShrinkImageAsync(stream, quality, ct);
            if (result.IsFailure) return Results.BadRequest(result.Error);

            return Results.File(result.Value, "image/jpeg", $"{file.FileName}_shrunk.jpg");
        }).DisableAntiforgery();

        group.MapPost("/image/preview", async (IFormFile file, IFileService fileService, CancellationToken ct = default) =>
        {
            using var stream = file.OpenReadStream();
            var result = await fileService.GenerateImagePreviewAsync(stream, ct);
            if (result.IsFailure) return Results.BadRequest(result.Error);

            return Results.File(result.Value, "image/jpeg", $"{file.FileName}_preview.jpg");
        }).DisableAntiforgery();

        // ── Video / Audio Processing ──────────────────────────────────────────

        group.MapPost("/video/preview", async (string fileId, IFileService fileService, CancellationToken ct = default) =>
        {
            var result = await fileService.GenerateVideoPreviewAsync(fileId, ct);
            if (result.IsFailure) return Results.BadRequest(result.Error);

            return Results.File(result.Value, "image/jpeg", $"{fileId}_preview.jpg");
        });

        group.MapPost("/video/compress", async (string fileId, IFileService fileService, CancellationToken ct = default) =>
        {
            var result = await fileService.CompressVideoAsync(fileId, ct);
            if (result.IsFailure) return Results.BadRequest(result.Error);

            return Results.Ok(new { CompressedFileId = result.Value, Message = "Video compressed successfully. You can download it via /api/v1/files/download." });
        });

        group.MapPost("/audio/compress", async (string fileId, IFileService fileService, CancellationToken ct = default) =>
        {
            var result = await fileService.CompressAudioAsync(fileId, ct);
            if (result.IsFailure) return Results.BadRequest(result.Error);

            return Results.Ok(new { CompressedFileId = result.Value, Message = "Audio compressed successfully. You can download it via /api/v1/files/download." });
        });

        // ── Document Processing ───────────────────────────────────────────────

        group.MapPost("/document/preview", async (string fileId, IFileService fileService, CancellationToken ct = default) =>
        {
            var result = await fileService.GenerateDocumentPreviewAsync(fileId, ct);
            if (result.IsFailure) return Results.BadRequest(result.Error);

            return Results.File(result.Value, "image/jpeg");
        });
    }
}

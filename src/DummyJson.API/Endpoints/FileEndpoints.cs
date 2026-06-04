using System.Threading;
using DummyJson.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace DummyJson.API.Endpoints;

public static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/files").WithTags("Files");

        // ── Normal File Upload ────────────────────────────────────────────────
        
        group.MapPost("/upload", async (IFormFile file, IFileService fileService, CancellationToken ct) =>
        {
            using var stream = file.OpenReadStream();
            var result = await fileService.UploadFileAsync(stream, file.FileName, file.ContentType, file.Length, options: null, cancellationToken: ct);
            if (result.IsFailure) return Results.BadRequest(result.Error);

            return Results.Ok(new { FileId = result.Value, Message = "File uploaded successfully." });
        }).DisableAntiforgery(); // Required for IFormFile in Minimal APIs

        // ── Download File ─────────────────────────────────────────────────────

        group.MapGet("/download/{fileId}", (string fileId, IFileService fileService) =>
        {
            var result = fileService.GetFilePath(fileId);
            if (result.IsFailure) return Results.NotFound(result.Error);

            var contentType = "application/octet-stream";
            return Results.File(result.Value, contentType, fileDownloadName: fileId);
        });

        // ── Stream Video ──────────────────────────────────────────────────────

        group.MapGet("/stream/{fileId}", (string fileId, IFileService fileService) =>
        {
            var result = fileService.GetFilePath(fileId);
            if (result.IsFailure) return Results.NotFound(result.Error);

            // Using File with enableRangeProcessing = true enables HTTP 206 Partial Content (perfect for video streaming)
            var contentType = fileId.EndsWith(".mp4") ? "video/mp4" : "application/octet-stream";
            return Results.File(result.Value, contentType, enableRangeProcessing: true);
        });
    }
}

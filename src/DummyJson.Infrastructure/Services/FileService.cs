using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using SharedKernel.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using FFMpegCore;

namespace DummyJson.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadDirectory;
    private const long DefaultMaxFileSize = 100 * 1024 * 1024; // 100 MB

    // Default configurations
    private readonly string[] _defaultImageExts = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private readonly string[] _defaultVideoExts = { ".mp4", ".avi", ".mov", ".mkv" };
    private readonly string[] _defaultAudioExts = { ".mp3", ".wav", ".ogg" };
    private readonly string[] _defaultDocumentExts = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

    public FileService(IConfiguration configuration)
    {
        _uploadDirectory = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadDirectory))
            Directory.CreateDirectory(_uploadDirectory);
    }

    public async Task<Result<string>> UploadFileAsync(Stream content, string fileName, string contentType, long length, FileUploadOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (content == null || length == 0)
            return Result.Failure<string>(Error.Validation("File.Empty", "File cannot be empty."));

        if (options?.AllowAll != true)
        {
            var maxFileSize = options?.MaxFileSize ?? DefaultMaxFileSize;
            if (length > maxFileSize)
                return Result.Failure<string>(Error.Validation("File.TooLarge", $"File size exceeds the {maxFileSize / 1024 / 1024} MB limit."));

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var allowedExtensions = options?.AllowedExtensions ?? _defaultImageExts.Concat(_defaultVideoExts).Concat(_defaultAudioExts).Concat(_defaultDocumentExts).ToArray();
            
            if (!allowedExtensions.Contains("*") && !allowedExtensions.Contains(extension))
                return Result.Failure<string>(Error.Validation("File.InvalidExtension", $"Extension {extension} is not allowed."));

            var allowedMimeTypes = options?.AllowedMimeTypes;
            if (allowedMimeTypes != null && !allowedMimeTypes.Contains("*") && !allowedMimeTypes.Contains(contentType.ToLowerInvariant()))
                return Result.Failure<string>(Error.Validation("File.InvalidMimeType", $"MimeType {contentType} is not allowed."));
        }

        var extensionFinal = Path.GetExtension(fileName).ToLowerInvariant();
        var fileId = Guid.NewGuid().ToString() + extensionFinal;
        var filePath = Path.Combine(_uploadDirectory, fileId);

        using var stream = new FileStream(filePath, FileMode.Create);
        await content.CopyToAsync(stream, cancellationToken);

        return Result.Success(fileId);
    }

    public Result<string> GetFilePath(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            return Result.Failure<string>(Error.Validation("File.InvalidId", "File ID is invalid."));

        if (fileId.Contains("..") || fileId.Contains("/") || fileId.Contains("\\"))
            return Result.Failure<string>(Error.Validation("File.SecurityError", "Invalid file name."));

        var path = Path.Combine(_uploadDirectory, fileId);
        if (!File.Exists(path))
            return Result.Failure<string>(Error.NotFound("File.NotFound", "The requested file was not found."));

        return Result.Success(path);
    }

    public Result<Stream> GetFileStream(string fileId)
    {
        var pathResult = GetFilePath(fileId);
        if (pathResult.IsFailure)
            return Result.Failure<Stream>(pathResult.Error);

        var stream = new FileStream(pathResult.Value, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Result.Success((Stream)stream);
    }

    // ── Media Processing ──────────────────────────────────────────────────────

    public async Task<Result<Stream>> ResizeImageAsync(Stream imageStream, int width, int height, CancellationToken cancellationToken = default)
    {
        try
        {
            using var image = await Image.LoadAsync(imageStream, cancellationToken);
            image.Mutate(x => x.Resize(width, height));
            
            var outStream = new MemoryStream();
            await image.SaveAsJpegAsync(outStream, cancellationToken);
            outStream.Position = 0;
            return Result.Success((Stream)outStream);
        }
        catch (Exception ex)
        {
            return Result.Failure<Stream>(Error.Failure("Image.ResizeError", $"Failed to resize image: {ex.Message}"));
        }
    }

    public async Task<Result<Stream>> ShrinkImageAsync(Stream imageStream, int quality = 75, CancellationToken cancellationToken = default)
    {
        try
        {
            using var image = await Image.LoadAsync(imageStream, cancellationToken);
            
            var outStream = new MemoryStream();
            var encoder = new JpegEncoder { Quality = quality };
            await image.SaveAsJpegAsync(outStream, encoder, cancellationToken);
            outStream.Position = 0;
            return Result.Success((Stream)outStream);
        }
        catch (Exception ex)
        {
            return Result.Failure<Stream>(Error.Failure("Image.ShrinkError", $"Failed to shrink image: {ex.Message}"));
        }
    }

    public async Task<Result<Stream>> GenerateImagePreviewAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        // Preview is just a tiny thumbnail (e.g., 100x100)
        return await ResizeImageAsync(imageStream, 100, 100, cancellationToken);
    }

    public async Task<Result<string>> CompressVideoAsync(string videoFileId, CancellationToken cancellationToken = default)
    {
        var pathResult = GetFilePath(videoFileId);
        if (pathResult.IsFailure) return Result.Failure<string>(pathResult.Error);

        var outFileName = Guid.NewGuid().ToString() + "_compressed.mp4";
        var outPath = Path.Combine(_uploadDirectory, outFileName);

        try
        {
            // Requires ffmpeg on server
            var success = await FFMpegArguments
                .FromFileInput(pathResult.Value)
                .OutputToFile(outPath, true, options => options
                    .WithVideoCodec(FFMpegCore.Enums.VideoCodec.LibX264)
                    .WithAudioCodec(FFMpegCore.Enums.AudioCodec.Aac)
                    .WithFastStart())
                .ProcessAsynchronously();

            if (!success)
                return Result.Failure<string>(Error.Failure("Video.CompressError", "FFmpeg failed to compress video."));

            return Result.Success(outFileName);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Failure("Video.CompressError", $"Ensure FFmpeg is installed. Error: {ex.Message}"));
        }
    }

    public async Task<Result<Stream>> GenerateVideoPreviewAsync(string videoFileId, CancellationToken cancellationToken = default)
    {
        var pathResult = GetFilePath(videoFileId);
        if (pathResult.IsFailure) return Result.Failure<Stream>(pathResult.Error);

        var outFileName = Guid.NewGuid().ToString() + "_preview.jpg";
        var outPath = Path.Combine(_uploadDirectory, outFileName);

        try
        {
            // Extract the first frame
            var success = await FFMpegArguments
                .FromFileInput(pathResult.Value)
                .OutputToFile(outPath, true, options => options
                    .Seek(TimeSpan.FromSeconds(1))
                    .WithCustomArgument("-vframes 1"))
                .ProcessAsynchronously();

            if (!success || !File.Exists(outPath))
                return Result.Failure<Stream>(Error.Failure("Video.PreviewError", "FFmpeg failed to generate preview."));

            var stream = new FileStream(outPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Result.Success((Stream)stream);
        }
        catch (Exception ex)
        {
            return Result.Failure<Stream>(Error.Failure("Video.PreviewError", $"Ensure FFmpeg is installed. Error: {ex.Message}"));
        }
    }

    public async Task<Result<string>> CompressAudioAsync(string audioFileId, CancellationToken cancellationToken = default)
    {
        var pathResult = GetFilePath(audioFileId);
        if (pathResult.IsFailure) return Result.Failure<string>(pathResult.Error);

        var outFileName = Guid.NewGuid().ToString() + "_compressed.mp3";
        var outPath = Path.Combine(_uploadDirectory, outFileName);

        try
        {
            var success = await FFMpegArguments
                .FromFileInput(pathResult.Value)
                .OutputToFile(outPath, true, options => options
                    .WithAudioCodec(FFMpegCore.Enums.AudioCodec.LibMp3Lame)
                    .WithAudioBitrate(128)) // 128k compression
                .ProcessAsynchronously();

            if (!success)
                return Result.Failure<string>(Error.Failure("Audio.CompressError", "FFmpeg failed to compress audio."));

            return Result.Success(outFileName);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Failure("Audio.CompressError", $"Ensure FFmpeg is installed. Error: {ex.Message}"));
        }
    }

    public Task<Result<Stream>> GenerateDocumentPreviewAsync(string documentFileId, CancellationToken cancellationToken = default)
    {
        // PDF to Image is complex without heavy native libraries (like Pdfium or Magick with Ghostscript).
        // Returning a placeholder image stream or a failure indicating it's unsupported.
        return Task.FromResult(Result.Failure<Stream>(Error.Failure("Document.Preview", "Document previews require an external renderer (e.g., PdfiumViewer or LibreOffice Headless). Returning generic document icon on frontend is recommended.")));
    }
}

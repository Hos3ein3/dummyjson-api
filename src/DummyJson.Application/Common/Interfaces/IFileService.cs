using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharedKernel.Results;

namespace DummyJson.Application.Common.Interfaces;

public class FileUploadOptions
{
    public long? MaxFileSize { get; set; }
    public string[]? AllowedExtensions { get; set; }
    public string[]? AllowedMimeTypes { get; set; }
    public bool AllowAll { get; set; } // If true, ignores validation
}

public interface IFileService
{
    /// <summary>
    /// Validates and uploads a file to the local directory.
    /// </summary>
    Task<Result<string>> UploadFileAsync(Stream content, string fileName, string contentType, long length, FileUploadOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the absolute physical path of a file by its identifier.
    /// </summary>
    Result<string> GetFilePath(string fileId);

    /// <summary>
    /// Reads the file as a stream.
    /// </summary>
    Result<Stream> GetFileStream(string fileId);

    // ── Media Processing ──────────────────────────────────────────────────────
    
    Task<Result<Stream>> ResizeImageAsync(Stream imageStream, int width, int height, CancellationToken cancellationToken = default);
    Task<Result<Stream>> ShrinkImageAsync(Stream imageStream, int quality = 75, CancellationToken cancellationToken = default);
    Task<Result<Stream>> GenerateImagePreviewAsync(Stream imageStream, CancellationToken cancellationToken = default);

    Task<Result<string>> CompressVideoAsync(string videoFileId, CancellationToken cancellationToken = default);
    Task<Result<Stream>> GenerateVideoPreviewAsync(string videoFileId, CancellationToken cancellationToken = default);
    
    Task<Result<string>> CompressAudioAsync(string audioFileId, CancellationToken cancellationToken = default);
    
    Task<Result<Stream>> GenerateDocumentPreviewAsync(string documentFileId, CancellationToken cancellationToken = default);
}

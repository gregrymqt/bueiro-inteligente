using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using backend.Core.Settings;
using backend.Features.Uploads.Application.Interfaces;
using backend.Features.Uploads.Domain;
using backend.Features.Uploads.Domain.Entities;
using backend.Features.Uploads.Domain.Interfaces;
using backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Supabase;

namespace backend.Features.Uploads.Application.Services;

public class UploadService : IUploadService
{
    private const string SupabaseBucketName = "bueiro_bucket";
    private const string WebpContentType = "image/webp";
    private const string WebpExtension = ".webp";
    private const int MaxImageWidth = 1920;
    private const int MaxImageHeight = 1920;
    private const int WebpQuality = 80;

    private readonly IUploadRepository _repository;
    private readonly ILogger<UploadService> _logger;
    private readonly string _storagePath;
    private readonly IOptions<SupabaseSettings> _supabaseOptions;
    private readonly Client? _supabaseClient;
    private readonly IUnitOfWork _unitOfWork;

    public UploadService(
        IUploadRepository repository,
        IConfiguration configuration,
        ILogger<UploadService> logger,
        IOptions<SupabaseSettings> supabaseOptions,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _logger = logger;
        _supabaseOptions = supabaseOptions;
        _unitOfWork = unitOfWork;

        _storagePath =
            configuration["UploadSettings:StoragePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        // Inicializa o cliente Supabase se configurado
        if (_supabaseOptions.Value.UseStorage)
        {
            var supabaseUrl =
                _supabaseOptions.Value.Url
                ?? throw new InvalidOperationException(
                    $"Missing Supabase URL in {SupabaseSettings.SectionName} configuration."
                );

            var supabaseKey =
                _supabaseOptions.Value.Key
                ?? throw new InvalidOperationException(
                    $"Missing Supabase Key in {SupabaseSettings.SectionName} configuration."
                );

            _supabaseClient = new Client(supabaseUrl, supabaseKey);
        }

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<UploadModel?> GetUploadByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<UploadModel>> GetAllUploadsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<UploadModel> ProcessUploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.");
        }

        if (
            string.IsNullOrWhiteSpace(file.ContentType)
            || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new ArgumentException("Invalid image file or content type mismatch.");
        }

        var sanitizedFileName = Path.GetFileName(file.FileName);
        var uploadId = Guid.NewGuid();
        var storedFileName = BuildStoredFileName(uploadId);
        var createdAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Starting upload processing for file {FileName} using {StorageMode} storage.",
            sanitizedFileName,
            _supabaseOptions.Value.UseStorage ? "Supabase" : "local"
        );

        byte[] webpBytes;

        try
        {
            webpBytes = await ConvertToWebpBytesAsync(file).ConfigureAwait(false);
        }
        catch (UnknownImageFormatException ex)
        {
            _logger.LogWarning(ex, "Invalid image format received for file {FileName}.", sanitizedFileName);
            throw new ArgumentException("Invalid image file or content type mismatch.", ex);
        }

        if (_supabaseOptions.Value.UseStorage)
        {
            return await ProcessUploadToSupabaseAsync(
                    webpBytes,
                    uploadId,
                    storedFileName,
                    createdAt
                )
                .ConfigureAwait(false);
        }

        return await ProcessLocalUploadAsync(
                webpBytes,
                uploadId,
                storedFileName,
                createdAt
            )
            .ConfigureAwait(false);
    }

    public async Task DeleteUploadAsync(Guid id)
    {
        var upload = await _repository.GetByIdAsync(id);
        if (upload != null)
        {
            if (_supabaseOptions.Value.UseStorage)
            {
                if (_supabaseClient is not null)
                {
                    await _supabaseClient
                        .Storage.From(SupabaseBucketName)
                        .Remove(upload.StoragePath)
                        .ConfigureAwait(false);
                }
            }
            else if (File.Exists(upload.StoragePath))
            {
                File.Delete(upload.StoragePath);
            }

            await _unitOfWork.ExecuteTransactionAsync(async ct =>
            {
                await _repository.DeleteAsync(id).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private async Task<UploadModel> ProcessLocalUploadAsync(
        byte[] webpBytes,
        Guid uploadId,
        string storedFileName,
        DateTime createdAt
    )
    {
        var filePath = Path.Combine(_storagePath, storedFileName);

        // Check disk space
        var driveInfo = new DriveInfo(
            Path.GetPathRoot(Path.GetFullPath(_storagePath)) ?? string.Empty
        );
        if (driveInfo.IsReady && driveInfo.AvailableFreeSpace < webpBytes.LongLength)
        {
            throw new IOException("Not enough disk space to save the file.");
        }

        try
        {
            const int bufferSize = 81920;
            await using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                FileOptions.Asynchronous
            );

            await stream.WriteAsync(webpBytes).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);

            var checksumHex = Convert.ToHexString(SHA256.HashData(webpBytes)).ToLowerInvariant();

            var uploadModel = new UploadModel
            {
                Id = uploadId,
                FileName = storedFileName,
                ContentType = WebpContentType,
                Size = webpBytes.LongLength,
                StoragePath = filePath,
                Extension = WebpExtension,
                Checksum = checksumHex,
                Url = BuildLocalUploadUrl(storedFileName),
                CreatedAt = createdAt,
            };

            await _unitOfWork.ExecuteTransactionAsync(async ct =>
            {
                await _repository.AddAsync(uploadModel).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return uploadModel;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(
                ex,
                "Unauthorized access while attempting to save file to {Path}",
                filePath
            );
            throw new IOException("Access to the storage path is denied.", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error occurred while saving file to {Path}", filePath);
            throw;
        }
    }

    private async Task<UploadModel> ProcessUploadToSupabaseAsync(
        byte[] webpBytes,
        Guid uploadId,
        string storedFileName,
        DateTime createdAt
    )
    {
        if (_supabaseClient is null)
        {
            throw new InvalidOperationException("Supabase client is not configured.");
        }

        var checksumHex = Convert.ToHexString(SHA256.HashData(webpBytes)).ToLowerInvariant();
        var storagePath = BuildSupabaseStoragePath(createdAt, storedFileName);

        try
        {
            var bucket = _supabaseClient.Storage.From(SupabaseBucketName);
            await bucket.Upload(webpBytes, storagePath).ConfigureAwait(false);

            var publicUrl = bucket.GetPublicUrl(storagePath);

            var uploadModel = new UploadModel
            {
                Id = uploadId,
                FileName = storedFileName,
                ContentType = WebpContentType,
                Size = webpBytes.LongLength,
                StoragePath = storagePath,
                Extension = WebpExtension,
                Checksum = checksumHex,
                Url = publicUrl,
                CreatedAt = createdAt,
            };

            await _unitOfWork.ExecuteTransactionAsync(async ct =>
            {
                await _repository.AddAsync(uploadModel).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return uploadModel;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(
                ex,
                "Unauthorized access while attempting to upload file {FileName} to Supabase path {StoragePath}",
                storedFileName,
                storagePath
            );
            throw new IOException("Access to Supabase Storage is denied.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while uploading file {FileName} to Supabase path {StoragePath}",
                storedFileName,
                storagePath
            );
            throw new IOException(
                "An error occurred while uploading the file to Supabase Storage.",
                ex
            );
        }
    }

    private static async Task<byte[]> ConvertToWebpBytesAsync(IFormFile file)
    {
        using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream).ConfigureAwait(false);

        ResizeImageIfNeeded(image);

        using var outputStream = new MemoryStream();
        var encoder = new WebpEncoder
        {
            FileFormat = WebpFileFormatType.Lossy,
            Quality = WebpQuality,
        };

        await image.SaveAsWebpAsync(outputStream, encoder).ConfigureAwait(false);
        return outputStream.ToArray();
    }

    private static void ResizeImageIfNeeded(Image image)
    {
        if (image.Width <= MaxImageWidth && image.Height <= MaxImageHeight)
        {
            return;
        }

        image.Mutate(context =>
            context.Resize(
                new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(MaxImageWidth, MaxImageHeight),
                }
            )
        );
    }

    private static string BuildLocalUploadUrl(string fileName)
    {
        return $"/uploads/{fileName}";
    }

    private static string BuildStoredFileName(Guid uploadId)
    {
        return $"{uploadId}{WebpExtension}";
    }

    private static string BuildSupabaseStoragePath(DateTime createdAt, string storedFileName)
    {
        var year = createdAt.ToString("yyyy", CultureInfo.InvariantCulture);
        var month = createdAt.ToString("MM", CultureInfo.InvariantCulture);
        return $"uploads/{year}/{month}/{storedFileName}";
    }
}

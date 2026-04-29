using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using backend.Core.Settings;
using backend.Features.Uploads.Domain;
using backend.Features.Uploads.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Supabase;

namespace backend.Features.Uploads.Application.Services;

public class UploadService : IUploadService
{
    private const string SupabaseBucketName = "bueiro_bucket";

    private readonly IUploadRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UploadService> _logger;
    private readonly string _storagePath;
    private readonly IOptions<SupabaseSettings> _supabaseOptions;
    private readonly Client? _supabaseClient;

    public UploadService(
        IUploadRepository repository,
        IConfiguration configuration,
        ILogger<UploadService> logger,
        IOptions<SupabaseSettings> supabaseOptions
    )
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
        _supabaseOptions = supabaseOptions;

        _storagePath =
            _configuration["UploadSettings:StoragePath"]
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

        if (!IsValidImage(file))
        {
            throw new ArgumentException("Invalid image file or content type mismatch.");
        }

        _logger.LogInformation(
            "Starting upload processing for file {FileName} using {StorageMode} storage.",
            file.FileName,
            _supabaseOptions.Value.UseStorage ? "Supabase" : "local"
        );

        // Sanitize file name (Path Traversal protection)
        var sanitizedFileName = Path.GetFileName(file.FileName);

        var uploadId = Guid.NewGuid();
        var extension = Path.GetExtension(sanitizedFileName);
        var createdAt = DateTime.UtcNow;

        if (_supabaseOptions.Value.UseStorage)
        {
            return await ProcessUploadToSupabaseAsync(
                    file,
                    uploadId,
                    sanitizedFileName,
                    extension,
                    createdAt
                )
                .ConfigureAwait(false);
        }

        return await ProcessLocalUploadAsync(
                file,
                uploadId,
                sanitizedFileName,
                extension,
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

            await _repository.DeleteAsync(id);
        }
    }

    private async Task<UploadModel> ProcessLocalUploadAsync(
        IFormFile file,
        Guid uploadId,
        string sanitizedFileName,
        string extension,
        DateTime createdAt
    )
    {
        var storedFileName = $"{uploadId}{extension}";
        var filePath = Path.Combine(_storagePath, storedFileName);

        // Check disk space
        var driveInfo = new DriveInfo(
            Path.GetPathRoot(Path.GetFullPath(_storagePath)) ?? string.Empty
        );
        if (driveInfo.IsReady && driveInfo.AvailableFreeSpace < file.Length)
        {
            throw new IOException("Not enough disk space to save the file.");
        }

        try
        {
            // Optimize FileStream with larger buffer and asynchronous flag
            const int bufferSize = 81920; // 80 KB
            await using var stream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                FileOptions.Asynchronous
            );

            // Compute Checksum while saving
            using var sha256 = SHA256.Create();
            await using var cryptoStream = new CryptoStream(stream, sha256, CryptoStreamMode.Write);

            await file.CopyToAsync(cryptoStream);

            // Ensure all data is written and hashes computed
            await cryptoStream.FlushFinalBlockAsync();
            var checksumBytes =
                sha256.Hash
                ?? throw new InvalidOperationException("Unable to compute file checksum.");
            var checksumHex = Convert.ToHexString(checksumBytes).ToLowerInvariant();

            var uploadModel = new UploadModel
            {
                Id = uploadId,
                FileName = sanitizedFileName,
                ContentType = file.ContentType,
                Size = file.Length,
                StoragePath = filePath,
                Extension = extension,
                Checksum = checksumHex,
                Url = BuildLocalUploadUrl(storedFileName),
                CreatedAt = createdAt,
            };

            return await _repository.AddAsync(uploadModel);
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
        IFormFile file,
        Guid uploadId,
        string sanitizedFileName,
        string extension,
        DateTime createdAt
    )
    {
        if (_supabaseClient is null)
        {
            throw new InvalidOperationException("Supabase client is not configured.");
        }

        var fileBytes = await ReadFileBytesAsync(file);
        var checksumHex = Convert.ToHexString(SHA256.HashData(fileBytes)).ToLowerInvariant();
        var storagePath = BuildSupabaseStoragePath(createdAt, uploadId, extension);

        try
        {
            var bucket = _supabaseClient.Storage.From(SupabaseBucketName);
            await bucket.Upload(fileBytes, storagePath).ConfigureAwait(false);

            var publicUrl = bucket.GetPublicUrl(storagePath);

            var uploadModel = new UploadModel
            {
                Id = uploadId,
                FileName = sanitizedFileName,
                ContentType = file.ContentType,
                Size = file.Length,
                StoragePath = storagePath,
                Extension = extension,
                Checksum = checksumHex,
                Url = publicUrl,
                CreatedAt = createdAt,
            };

            return await _repository.AddAsync(uploadModel);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(
                ex,
                "Unauthorized access while attempting to upload file {FileName} to Supabase path {StoragePath}",
                sanitizedFileName,
                storagePath
            );
            throw new IOException("Access to Supabase Storage is denied.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while uploading file {FileName} to Supabase path {StoragePath}",
                sanitizedFileName,
                storagePath
            );
            throw new IOException(
                "An error occurred while uploading the file to Supabase Storage.",
                ex
            );
        }
    }

    private bool IsValidImage(IFormFile file)
    {
        if (file.Length < 4)
        {
            return false;
        }

        var headerBytes = new byte[4];
        using (var stream = file.OpenReadStream())
        {
            stream.ReadExactly(headerBytes, 0, 4);
        }

        var headerHex = BitConverter.ToString(headerBytes).Replace("-", "").ToUpperInvariant();
        var contentType = file.ContentType.ToLowerInvariant();

        if (
            headerHex.StartsWith("FFD8FF")
            && (contentType == "image/jpeg" || contentType == "image/jpg")
        )
            return true;
        if (headerHex.StartsWith("89504E47") && contentType == "image/png")
            return true;
        if (headerHex.StartsWith("47494638") && contentType == "image/gif")
            return true;
        if (headerHex.StartsWith("424D") && contentType == "image/bmp")
            return true;
        return headerHex.StartsWith("52494646") && contentType == "image/webp";
    }

    private static async Task<byte[]> ReadFileBytesAsync(IFormFile file)
    {
        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private static string BuildLocalUploadUrl(string fileName)
    {
        return $"/uploads/{fileName}";
    }

    private static string BuildSupabaseStoragePath(
        DateTime createdAt,
        Guid uploadId,
        string extension
    )
    {
        var year = createdAt.ToString("yyyy", CultureInfo.InvariantCulture);
        var month = createdAt.ToString("MM", CultureInfo.InvariantCulture);
        return $"uploads/{year}/{month}/{uploadId}{extension}";
    }
}

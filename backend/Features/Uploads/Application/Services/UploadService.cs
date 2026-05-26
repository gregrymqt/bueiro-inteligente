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
using backend.Features.Uploads.Domain.Enums;
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
    private const int DesktopWidth = 1920;
    private const int DesktopHeight = 1080;
    private const int MobileWidth = 640;
    private const int MobileHeight = 420;
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

        if (_supabaseOptions.Value.UseStorage)
        {
            var supabaseUrl =
                _supabaseOptions.Value.Url
                ?? throw new InvalidOperationException($"Missing Supabase URL in {SupabaseSettings.SectionName} configuration.");

            var supabaseKey =
                _supabaseOptions.Value.Key
                ?? throw new InvalidOperationException($"Missing Supabase Key in {SupabaseSettings.SectionName} configuration.");

            _supabaseClient = new Client(supabaseUrl, supabaseKey);
        }

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<UploadModel?> GetUploadByIdAsync(Guid id) => await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<UploadModel>> GetAllUploadsAsync() => await _repository.GetAllAsync();

    public async Task<(UploadModel Desktop, UploadModel Mobile)> ProcessUploadAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) throw new ArgumentException("File is empty or null.");
        if (string.IsNullOrWhiteSpace(file.ContentType) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid image file or content type mismatch.");

        var sanitizedFileName = Path.GetFileName(file.FileName);
        var createdAt = DateTime.UtcNow;

        _logger.LogInformation("Processing file {FileName}", sanitizedFileName);

        byte[] originalBytes;
        using (var memStream = new MemoryStream())
        {
            await file.CopyToAsync(memStream);
            originalBytes = memStream.ToArray();
        }

        var desktopBytes = await ConvertToWebpBytesAsync(originalBytes, DesktopWidth, DesktopHeight).ConfigureAwait(false);
        var mobileBytes = await ConvertToWebpBytesAsync(originalBytes, MobileWidth, MobileHeight).ConfigureAwait(false);

        var desktopId = Guid.NewGuid();
        var mobileId = Guid.NewGuid();

        var desktopFileName = $"{desktopId}_desktop{WebpExtension}";
        var mobileFileName = $"{mobileId}_mobile{WebpExtension}";

        UploadModel desktopModel, mobileModel;

        if (_supabaseOptions.Value.UseStorage)
        {
            desktopModel = await ProcessUploadToSupabaseAsync(desktopBytes, desktopId, desktopFileName, createdAt, DeviceType.Desktop).ConfigureAwait(false);
            mobileModel = await ProcessUploadToSupabaseAsync(mobileBytes, mobileId, mobileFileName, createdAt, DeviceType.Mobile).ConfigureAwait(false);
        }
        else
        {
            desktopModel = await ProcessLocalUploadAsync(desktopBytes, desktopId, desktopFileName, createdAt, DeviceType.Desktop).ConfigureAwait(false);
            mobileModel = await ProcessLocalUploadAsync(mobileBytes, mobileId, mobileFileName, createdAt, DeviceType.Mobile).ConfigureAwait(false);
        }

        await _unitOfWork.ExecuteTransactionAsync(async ct =>
        {
            await _repository.AddAsync(desktopModel).ConfigureAwait(false);
            await _repository.AddAsync(mobileModel).ConfigureAwait(false);
        }).ConfigureAwait(false);

        return (desktopModel, mobileModel);
    }

    public async Task DeleteUploadAsync(Guid id)
    {
        var upload = await _repository.GetByIdAsync(id);
        if (upload != null)
        {
            if (_supabaseOptions.Value.UseStorage && _supabaseClient is not null)
            {
                await _supabaseClient.Storage.From(SupabaseBucketName).Remove(upload.StoragePath).ConfigureAwait(false);
            }
            else if (File.Exists(upload.StoragePath))
            {
                File.Delete(upload.StoragePath);
            }

            await _unitOfWork.ExecuteTransactionAsync(async ct => {
                await _repository.DeleteAsync(id).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }

    private async Task<UploadModel> ProcessLocalUploadAsync(byte[] webpBytes, Guid uploadId, string storedFileName, DateTime createdAt, DeviceType deviceType)
    {
        var filePath = Path.Combine(_storagePath, storedFileName);
        try
        {
            await File.WriteAllBytesAsync(filePath, webpBytes).ConfigureAwait(false);
            var checksumHex = Convert.ToHexString(SHA256.HashData(webpBytes)).ToLowerInvariant();

            var uploadModel = new UploadModel
            {
                Id = uploadId, FileName = storedFileName, ContentType = WebpContentType,
                Size = webpBytes.LongLength, StoragePath = filePath, Extension = WebpExtension,
                Checksum = checksumHex, Url = $"/uploads/{storedFileName}", CreatedAt = createdAt, DeviceType = deviceType
            };

            return uploadModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file locally");
            throw;
        }
    }

    private async Task<UploadModel> ProcessUploadToSupabaseAsync(byte[] webpBytes, Guid uploadId, string storedFileName, DateTime createdAt, DeviceType deviceType)
    {
        if (_supabaseClient is null) throw new InvalidOperationException("Supabase client is not configured.");

        var checksumHex = Convert.ToHexString(SHA256.HashData(webpBytes)).ToLowerInvariant();
        var storagePath = $"uploads/{createdAt:yyyy}/{createdAt:MM}/{storedFileName}";

        try
        {
            var bucket = _supabaseClient.Storage.From(SupabaseBucketName);
            await bucket.Upload(webpBytes, storagePath).ConfigureAwait(false);
            var publicUrl = bucket.GetPublicUrl(storagePath);

            var uploadModel = new UploadModel
            {
                Id = uploadId, FileName = storedFileName, ContentType = WebpContentType,
                Size = webpBytes.LongLength, StoragePath = storagePath, Extension = WebpExtension,
                Checksum = checksumHex, Url = publicUrl, CreatedAt = createdAt, DeviceType = deviceType
            };

            return uploadModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading to supabase");
            throw;
        }
    }

    private static async Task<byte[]> ConvertToWebpBytesAsync(byte[] imageBytes, int targetWidth, int targetHeight)
    {
        using var memoryStream = new MemoryStream(imageBytes);
        using var image = await Image.LoadAsync(memoryStream).ConfigureAwait(false);
        image.Mutate(context => context.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Size = new Size(targetWidth, targetHeight) }));

        using var outputStream = new MemoryStream();
        var encoder = new WebpEncoder { FileFormat = WebpFileFormatType.Lossy, Quality = WebpQuality };
        await image.SaveAsWebpAsync(outputStream, encoder).ConfigureAwait(false);
        return outputStream.ToArray();
    }
}

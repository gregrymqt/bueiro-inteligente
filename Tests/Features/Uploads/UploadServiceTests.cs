using System.Text;
using backend.Core.Settings;
using backend.Features.Uploads.Application.Services;
using backend.Features.Uploads.Domain.Entities;
using backend.Features.Uploads.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace backend.Tests.Features.Uploads;

public sealed class UploadServiceTests : IDisposable
{
    private readonly Mock<IUploadRepository> _repositoryMock = new();
    private readonly string _storagePath;
    private readonly UploadService _service;

    public UploadServiceTests()
    {
        _storagePath = Path.Combine(
            Path.GetTempPath(),
            "bueiro-inteligente-tests",
            Guid.NewGuid().ToString("N")
        );

        Directory.CreateDirectory(_storagePath);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["UploadSettings:StoragePath"] = _storagePath,
                }
            )
            .Build();

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<UploadModel>()))
            .ReturnsAsync((UploadModel upload) => upload);

        _service = new UploadService(
            _repositoryMock.Object,
            configuration,
            Mock.Of<ILogger<UploadService>>(),
            Options.Create(new SupabaseSettings { UseStorage = false })
        );
    }

    public void Dispose()
    {
        if (Directory.Exists(_storagePath))
        {
            Directory.Delete(_storagePath, recursive: true);
        }
    }

    [Fact]
    public async Task ProcessUploadAsync_DeveConverterImagemParaWebPESalvarNoDisco()
    {
        // Arrange
        using var inputStream = new MemoryStream();
        using (var image = new Image<Rgba32>(2400, 1600))
        {
            image.SaveAsPng(inputStream);
        }

        inputStream.Position = 0;
        var file = new FormFile(inputStream, 0, inputStream.Length, "file", "source.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png",
        };

        // Act
        var result = await _service.ProcessUploadAsync(file);

        // Assert
        result.FileName.Should().EndWith(".webp");
        result.ContentType.Should().Be("image/webp");
        result.Extension.Should().Be(".webp");
        result.Url.Should().Be($"/uploads/{result.FileName}");
        Path.GetFileName(result.StoragePath).Should().Be(result.FileName);
        Guid.TryParse(Path.GetFileNameWithoutExtension(result.FileName), out _).Should().BeTrue();

        var storedBytes = await File.ReadAllBytesAsync(result.StoragePath);
        storedBytes.Take(4).Should().Equal(new byte[] { 0x52, 0x49, 0x46, 0x46 });
        Encoding.ASCII.GetString(storedBytes, 8, 4).Should().Be("WEBP");
        result.Size.Should().Be(storedBytes.LongLength);

        using var storedStream = File.OpenRead(result.StoragePath);
        using var storedImage = await Image.LoadAsync(storedStream);

        storedImage.Width.Should().Be(1920);
        storedImage.Height.Should().Be(1280);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UploadModel>()), Times.Once);
    }

    [Fact]
    public async Task ProcessUploadAsync_ComArquivoInvalido_DeveLancarArgumentException()
    {
        // Arrange
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("not-an-image"));
        var file = new FormFile(inputStream, 0, inputStream.Length, "file", "invalid.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png",
        };

        // Act
        Func<Task> act = () => _service.ProcessUploadAsync(file);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UploadModel>()), Times.Never);
    }
}

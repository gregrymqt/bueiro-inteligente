using backend.Features.Monitoring.Domain.Entities;
using backend.Features.Monitoring.Infrastructure.Persistence.Repositories;

namespace backend.Tests.Features.Monitoring;

public sealed class CachedMonitoringRepositoryTests
{
    private readonly Mock<IMonitoringRepository> _repositoryMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CachedMonitoringRepository _repository;

    public CachedMonitoringRepositoryTests()
    {
        _repository = new CachedMonitoringRepository(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetLatestStatusAsync_CacheHit_NaoDeveConsultarRepositorio()
    {
        // Arrange
        const string drainId = "DRN-01";
        var status = new DrainStatusDTO(drainId, 90, 25, "Normal", -23.9, -46.3, DateTimeOffset.UtcNow);

        _cacheMock
            .Setup(c =>
                c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<DrainStatusDTO>>>(),
                    It.IsAny<TimeSpan?>()
                )
            )
            .ReturnsAsync(new CacheResponseDto<DrainStatusDTO>(status, true));

        // Act
        var result = await _repository.GetLatestStatusAsync(drainId);

        // Assert
        result.Should().BeEquivalentTo(status);
        _repositoryMock.Verify(
            r => r.GetLatestStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task GetLatestStatusAsync_CacheMiss_DeveConsultarRepositorio()
    {
        // Arrange
        const string drainId = "DRN-02";
        var status = new DrainStatusDTO(drainId, 88, 30, "Alerta", -23.9, -46.3, DateTimeOffset.UtcNow);

        _repositoryMock
            .Setup(r => r.GetLatestStatusAsync(drainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        _cacheMock
            .Setup(c =>
                c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<DrainStatusDTO>>>(),
                    It.IsAny<TimeSpan?>()
                )
            )
            .Returns(async (string key, Func<Task<DrainStatusDTO>> fetchFunc, TimeSpan? ttl) =>
                new CacheResponseDto<DrainStatusDTO>(await fetchFunc(), false)
            );

        // Act
        var result = await _repository.GetLatestStatusAsync(drainId);

        // Assert
        result.Should().BeEquivalentTo(status);
        _repositoryMock.Verify(
            r => r.GetLatestStatusAsync(drainId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task InsertAsync_DeveAtualizarCacheDoStatusAtual()
    {
        // Arrange
        var entity = new DrainStatus
        {
            DrainIdentifier = "DRN-03",
            DistanceCm = 42,
            ObstructionLevel = 65,
            Status = "Alerta",
            Latitude = -23.9,
            Longitude = -46.3,
            LastUpdate = new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero),
            DataHash = "HASH-123",
        };

        _repositoryMock
            .Setup(r => r.InsertAsync(entity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<DrainStatusDTO>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.InsertAsync(entity);

        // Assert
        _repositoryMock.Verify(
            r => r.InsertAsync(entity, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _cacheMock.Verify(
            c =>
                c.SetAsync(
                    "bueiro:DRN-03:status",
                    It.Is<DrainStatusDTO>(dto =>
                        dto.IdBueiro == entity.DrainIdentifier
                        && dto.DistanciaCm == entity.DistanceCm
                        && dto.NivelObstrucao == entity.ObstructionLevel
                        && dto.Status == entity.Status
                        && dto.Latitude == entity.Latitude
                        && dto.Longitude == entity.Longitude
                        && dto.UltimaAtualizacao == entity.LastUpdate
                        && dto.DataHash == entity.DataHash
                    ),
                    It.Is<TimeSpan?>(ttl => ttl == TimeSpan.FromHours(1))
                ),
            Times.Once
        );
    }
}
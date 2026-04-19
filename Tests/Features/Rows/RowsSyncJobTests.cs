using backend.Core.Settings;
using backend.Features.Rows.Application.DTOs;
using backend.Features.Rows.Application.Jobs;
using backend.Features.Rows.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace backend.Tests.Features.Rows;

public sealed class RowsSyncJobTests
{
    private readonly Mock<IMonitoringRepository> _monitoringRepoMock = new();
    private readonly Mock<IRowsService> _rowsServiceMock = new();
    private readonly RowsSettings _settings;
    private readonly RowsSyncJob _job;

    public RowsSyncJobTests()
    {
        _settings = CreateSettings();
        _job = new RowsSyncJob(
            SetupScope(),
            Options.Create(_settings),
            Mock.Of<ILogger<RowsSyncJob>>()
        );
    }

    #region Helpers (O gabarito de infraestrutura para o Job)

    private IServiceScopeFactory SetupScope()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(p => p.GetService(typeof(IMonitoringRepository)))
            .Returns(_monitoringRepoMock.Object);
        serviceProviderMock
            .Setup(p => p.GetService(typeof(IRowsService)))
            .Returns(_rowsServiceMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
        return scopeFactoryMock.Object;
    }

    private DrainStatusDTO CreateStatus(string id = "DRN-01") =>
        new(id, 50, 50, "Alerta", -180, -46.3, DateTimeOffset.UtcNow);

    #endregion

    [Fact]
    public async Task Execute_ComDadosPendentes_DeveSincronizarEMarcar()
    {
        // Arrange
        var data = new List<DrainStatusDTO> { CreateStatus("DRN-01"), CreateStatus("DRN-02") };
        _monitoringRepoMock
            .Setup(r => r.GetUnsyncedDataAsync(It.IsAny<int>(), default))
            .ReturnsAsync(data);
        _rowsServiceMock
            .Setup(s =>
                s.AppendDataAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<RowsAppendRequest>(),
                    default
                )
            )
            .ReturnsAsync(true);

        // Act
        await _job.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _rowsServiceMock.Verify(
            s =>
                s.AppendDataAsync(
                    _settings.SpreadsheetId,
                    _settings.TableId,
                    It.IsAny<RowsAppendRequest>(),
                    default
                ),
            Times.Once
        );
        _monitoringRepoMock.Verify(
            r =>
                r.MarkAsSyncedAsync(It.Is<IReadOnlyCollection<string>>(c => c.Count == 2), default),
            Times.Once
        );
    }

    [Fact]
    public async Task Execute_QuandoFalhaNoRows_NaoDeveMarcarComoSincronizado()
    {
        // Arrange
        var data = new List<DrainStatusDTO> { CreateStatus() };
        _monitoringRepoMock
            .Setup(r => r.GetUnsyncedDataAsync(It.IsAny<int>(), default))
            .ReturnsAsync(data);
        _rowsServiceMock
            .Setup(s =>
                s.AppendDataAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<RowsAppendRequest>(),
                    default
                )
            )
            .ThrowsAsync(new ExternalApiException("Rows", "Erro"));

        // Act
        await _job.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _monitoringRepoMock.Verify(
            r => r.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), default),
            Times.Never
        );
    }

    [Fact]
    public async Task Execute_SemDados_NaoDeveChamarRows()
    {
        // Arrange
        _monitoringRepoMock
            .Setup(r => r.GetUnsyncedDataAsync(It.IsAny<int>(), default))
            .ReturnsAsync(new List<DrainStatusDTO>());

        // Act
        await _job.Execute(Mock.Of<IJobExecutionContext>());

        // Assert
        _rowsServiceMock.Verify(
            s =>
                s.AppendDataAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<RowsAppendRequest>(),
                    default
                ),
            Times.Never
        );
    }

    private static RowsSettings CreateSettings() =>
        new()
        {
            ApiKey = "rows-key",
            BaseUrl = "https://api.rows.com/v1",
            SpreadsheetId = "ss-123",
            TableId = "tb-456",
        };
}

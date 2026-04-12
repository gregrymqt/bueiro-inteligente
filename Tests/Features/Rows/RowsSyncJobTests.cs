using System.Globalization;
using System.Reflection;
using backend.Core;
using backend.Features.Monitoring.Application.DTOs;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Features.Rows.Application.DTOs;
using backend.Features.Rows.Application.Jobs;
using backend.Features.Rows.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace backend.Tests.Features.Rows;

public sealed class RowsSyncJobTests
{
    private readonly Mock<ILogger<RowsSyncJob>> _loggerMock = new(MockBehavior.Loose);

    [Fact]
    public async Task Execute_QuandoSincronizacaoFalha_NaoDeveMarcarComoSincronizado()
    {
        // Arrange
        AppSettings settings = CreateSettings("spreadsheet-123", "table-456");
        Mock<IMonitoringRepository> monitoringRepositoryMock = new(MockBehavior.Strict);
        Mock<IRowsService> rowsServiceMock = new(MockBehavior.Strict);

        IReadOnlyList<DrainStatusDTO> unsyncedData =
        [
            CreateDrainStatus("DRN-001", 40, 66.67, "Alerta", -23.5505, -46.6333, 0)
        ];

        monitoringRepositoryMock
            .Setup(repository => repository.GetUnsyncedDataAsync(500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unsyncedData);

        rowsServiceMock
            .Setup(service => service.AppendDataAsync(
                settings.RowsSpreadsheetId,
                settings.RowsTableId,
                It.IsAny<RowsAppendRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalApiException("Rows", "Falha simulada ao enviar lote."));

        monitoringRepositoryMock
            .Setup(repository => repository.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        (Mock<IServiceScopeFactory> serviceScopeFactoryMock, _) = CreateScopeInfrastructure(
            monitoringRepositoryMock.Object,
            rowsServiceMock.Object
        );

        RowsSyncJob job = new(serviceScopeFactoryMock.Object, settings, _loggerMock.Object);
        Mock<IJobExecutionContext> executionContextMock = CreateExecutionContext();

        // Act
        await job.Execute(executionContextMock.Object);

        // Assert
        monitoringRepositoryMock.Verify(
            repository => repository.GetUnsyncedDataAsync(500, It.IsAny<CancellationToken>()),
            Times.Once
        );
        rowsServiceMock.Verify(
            service => service.AppendDataAsync(
                settings.RowsSpreadsheetId,
                settings.RowsTableId,
                It.IsAny<RowsAppendRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
        monitoringRepositoryMock.Verify(
            repository => repository.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        rowsServiceMock.VerifyNoOtherCalls();
        monitoringRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_QuandoNaoExistemDados_NaoDeveChamarServicoRows()
    {
        // Arrange
        AppSettings settings = CreateSettings("spreadsheet-123", "table-456");
        Mock<IMonitoringRepository> monitoringRepositoryMock = new(MockBehavior.Strict);
        Mock<IRowsService> rowsServiceMock = new(MockBehavior.Strict);

        monitoringRepositoryMock
            .Setup(repository => repository.GetUnsyncedDataAsync(500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DrainStatusDTO>());

        monitoringRepositoryMock
            .Setup(repository => repository.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        (Mock<IServiceScopeFactory> serviceScopeFactoryMock, _) = CreateScopeInfrastructure(
            monitoringRepositoryMock.Object,
            rowsServiceMock.Object
        );

        RowsSyncJob job = new(serviceScopeFactoryMock.Object, settings, _loggerMock.Object);
        Mock<IJobExecutionContext> executionContextMock = CreateExecutionContext();

        // Act
        await job.Execute(executionContextMock.Object);

        // Assert
        monitoringRepositoryMock.Verify(
            repository => repository.GetUnsyncedDataAsync(500, It.IsAny<CancellationToken>()),
            Times.Once
        );
        monitoringRepositoryMock.Verify(
            repository => repository.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        rowsServiceMock.Verify(
            service => service.AppendDataAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RowsAppendRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never
        );
        rowsServiceMock.VerifyNoOtherCalls();
        monitoringRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_ComDadosPendentes_DeveChamarAppendEMarcarComoSincronizado()
    {
        // Arrange
        AppSettings settings = CreateSettings("spreadsheet-123", "table-456");
        Mock<IMonitoringRepository> monitoringRepositoryMock = new(MockBehavior.Strict);
        Mock<IRowsService> rowsServiceMock = new(MockBehavior.Strict);

        IReadOnlyList<DrainStatusDTO> unsyncedData =
        [
            CreateDrainStatus("DRN-001", 40, 66.67, "Alerta", -23.5505, -46.6333, 0),
            CreateDrainStatus("DRN-002", 20, 83.33, "Crítico", -23.5515, -46.6343, 1)
        ];

        monitoringRepositoryMock
            .Setup(repository => repository.GetUnsyncedDataAsync(500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unsyncedData);

        List<RowsAppendRequest> capturedPayloads = [];
        rowsServiceMock
            .Setup(service => service.AppendDataAsync(
                settings.RowsSpreadsheetId,
                settings.RowsTableId,
                It.IsAny<RowsAppendRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, RowsAppendRequest, CancellationToken>((_, _, payload, _) => capturedPayloads.Add(payload))
            .ReturnsAsync(true);

        List<IReadOnlyCollection<string>> markedBatches = [];
        monitoringRepositoryMock
            .Setup(repository => repository.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<string>, CancellationToken>((identifiers, _) => markedBatches.Add(identifiers.ToArray()))
            .Returns(Task.CompletedTask);

        (Mock<IServiceScopeFactory> serviceScopeFactoryMock, _) = CreateScopeInfrastructure(
            monitoringRepositoryMock.Object,
            rowsServiceMock.Object
        );

        RowsSyncJob job = new(serviceScopeFactoryMock.Object, settings, _loggerMock.Object);
        Mock<IJobExecutionContext> executionContextMock = CreateExecutionContext();

        // Act
        await job.Execute(executionContextMock.Object);

        // Assert
        capturedPayloads.Should().HaveCount(1);
        capturedPayloads[0].Values.Should().HaveCount(2);
        capturedPayloads[0].Values[0].Should().Equal(
            "DRN-001",
            40d,
            66.67d,
            "Alerta",
            -23.5505d,
            -46.6333d,
            unsyncedData[0].UltimaAtualizacao.ToString("O", CultureInfo.InvariantCulture)
        );
        capturedPayloads[0].Values[1].Should().Equal(
            "DRN-002",
            20d,
            83.33d,
            "Crítico",
            -23.5515d,
            -46.6343d,
            unsyncedData[1].UltimaAtualizacao.ToString("O", CultureInfo.InvariantCulture)
        );

        markedBatches.Should().HaveCount(1);
        markedBatches[0].Should().Equal("DRN-001", "DRN-002");

        monitoringRepositoryMock.Verify(
            repository => repository.GetUnsyncedDataAsync(500, It.IsAny<CancellationToken>()),
            Times.Once
        );
        rowsServiceMock.Verify(
            service => service.AppendDataAsync(
                settings.RowsSpreadsheetId,
                settings.RowsTableId,
                It.IsAny<RowsAppendRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once
        );
        monitoringRepositoryMock.Verify(
            repository => repository.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        rowsServiceMock.VerifyNoOtherCalls();
        monitoringRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_ComMuitosDados_DeveRespeitarOChunkSize()
    {
        // Arrange
        AppSettings settings = CreateSettings("spreadsheet-123", "table-456");
        Mock<IMonitoringRepository> monitoringRepositoryMock = new(MockBehavior.Strict);
        Mock<IRowsService> rowsServiceMock = new(MockBehavior.Strict);

        IReadOnlyList<DrainStatusDTO> firstChunk = BuildBatch(1, 500);
        IReadOnlyList<DrainStatusDTO> secondChunk = BuildBatch(501, 1);

        monitoringRepositoryMock
            .SetupSequence(repository => repository.GetUnsyncedDataAsync(500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstChunk)
            .ReturnsAsync(secondChunk);

        List<RowsAppendRequest> capturedPayloads = [];
        rowsServiceMock
            .Setup(service => service.AppendDataAsync(
                settings.RowsSpreadsheetId,
                settings.RowsTableId,
                It.IsAny<RowsAppendRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, RowsAppendRequest, CancellationToken>((_, _, payload, _) => capturedPayloads.Add(payload))
            .ReturnsAsync(true);

        List<IReadOnlyCollection<string>> markedBatches = [];
        monitoringRepositoryMock
            .Setup(repository => repository.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<string>, CancellationToken>((identifiers, _) => markedBatches.Add(identifiers.ToArray()))
            .Returns(Task.CompletedTask);

        (Mock<IServiceScopeFactory> serviceScopeFactoryMock, _) = CreateScopeInfrastructure(
            monitoringRepositoryMock.Object,
            rowsServiceMock.Object
        );

        RowsSyncJob job = new(serviceScopeFactoryMock.Object, settings, _loggerMock.Object);
        Mock<IJobExecutionContext> executionContextMock = CreateExecutionContext();

        // Act
        await job.Execute(executionContextMock.Object);

        // Assert
        capturedPayloads.Should().HaveCount(2);
        capturedPayloads[0].Values.Should().HaveCount(500);
        capturedPayloads[1].Values.Should().HaveCount(1);

        markedBatches.Should().HaveCount(2);
        markedBatches[0].Should().HaveCount(500);
        markedBatches[1].Should().HaveCount(1);

        monitoringRepositoryMock.Verify(
            repository => repository.GetUnsyncedDataAsync(500, It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        rowsServiceMock.Verify(
            service => service.AppendDataAsync(
                settings.RowsSpreadsheetId,
                settings.RowsTableId,
                It.IsAny<RowsAppendRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        monitoringRepositoryMock.Verify(
            repository => repository.MarkAsSyncedAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
        rowsServiceMock.VerifyNoOtherCalls();
        monitoringRepositoryMock.VerifyNoOtherCalls();
    }

    private static AppSettings CreateSettings(string spreadsheetId, string tableId)
    {
        return new AppSettings(
            "Bueiro Inteligente",
            "1.0.0",
            "/api/v1",
            "secret-key",
            "HS256",
            30,
            "hardware-token",
            "redis://localhost:6379",
            true,
            true,
            "postgres://cloud",
            "postgres://local",
            "postgres://local",
            "migrations",
            "rows-api-key",
            "https://api.rows.com/v1",
            spreadsheetId,
            tableId,
            Array.Empty<string>(),
            false
        );
    }

    private static Mock<IJobExecutionContext> CreateExecutionContext()
    {
        Mock<IJobExecutionContext> executionContextMock = new(MockBehavior.Strict);
        executionContextMock.SetupGet(context => context.CancellationToken).Returns(CancellationToken.None);
        return executionContextMock;
    }

    private static (Mock<IServiceScopeFactory> ScopeFactoryMock, Mock<IServiceScope> ScopeMock) CreateScopeInfrastructure(
        IMonitoringRepository monitoringRepository,
        IRowsService rowsService
    )
    {
        Mock<IServiceProvider> serviceProviderMock = new(MockBehavior.Strict);
        serviceProviderMock
            .Setup(provider => provider.GetService(typeof(IMonitoringRepository)))
            .Returns(monitoringRepository);
        serviceProviderMock
            .Setup(provider => provider.GetService(typeof(IRowsService)))
            .Returns(rowsService);

        Mock<IServiceScope> scopeMock = new(MockBehavior.Strict);
        scopeMock.SetupGet(scope => scope.ServiceProvider).Returns(serviceProviderMock.Object);
        scopeMock.Setup(scope => scope.Dispose());

        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new(MockBehavior.Strict);
        serviceScopeFactoryMock.Setup(factory => factory.CreateScope()).Returns(scopeMock.Object);

        return (serviceScopeFactoryMock, scopeMock);
    }

    private static IReadOnlyList<DrainStatusDTO> BuildBatch(int startIndex, int count)
    {
        List<DrainStatusDTO> items = new(count);
        DateTimeOffset baseTimestamp = new(2026, 4, 11, 10, 0, 0, TimeSpan.Zero);

        for (int index = 0; index < count; index++)
        {
            int drainIndex = startIndex + index;

            items.Add(
                CreateDrainStatus(
                    $"DRN-{drainIndex:000}",
                    100 - drainIndex,
                    drainIndex % 2 == 0 ? 50d : 75d,
                    drainIndex % 2 == 0 ? "Alerta" : "Normal",
                    -23.5505d - (drainIndex / 10000d),
                    -46.6333d - (drainIndex / 10000d),
                    drainIndex - 1
                )
            );
        }

        return items;
    }

    private static DrainStatusDTO CreateDrainStatus(
        string drainIdentifier,
        double distanceCm,
        double obstructionLevel,
        string status,
        double latitude,
        double longitude,
        int minuteOffset
    )
    {
        return new DrainStatusDTO
        {
            IdBueiro = drainIdentifier,
            DistanciaCm = distanceCm,
            NivelObstrucao = obstructionLevel,
            Status = status,
            Latitude = latitude,
            Longitude = longitude,
            UltimaAtualizacao = new DateTimeOffset(2026, 4, 11, 10, 0, 0, TimeSpan.Zero).AddMinutes(minuteOffset),
        };
    }
}
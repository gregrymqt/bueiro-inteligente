namespace backend.Tests.Features.Drains;

public sealed class DrainServiceTests
{
    private readonly Mock<IDrainRepository> _repositoryMock = new(MockBehavior.Strict);
    private readonly DrainService _service;

    public DrainServiceTests()
    {
        _service = new DrainService(_repositoryMock.Object, Mock.Of<ILogger<DrainService>>());
    }

    [Fact]
    public async Task GetAllDrainsAsync_ComRegistrosExistentes_DeveRetornarListaDeDrainResponse()
    {
        // Arrange
        Guid firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Guid secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        DrainEntity firstDrain = BuildDrain(
            firstId,
            "Bueiro 1",
            "Rua A",
            10,
            20,
            "HW-1",
            true,
            new DateTimeOffset(2026, 4, 11, 10, 0, 0, TimeSpan.Zero)
        );

        DrainEntity secondDrain = BuildDrain(
            secondId,
            "Bueiro 2",
            "Rua B",
            30,
            40,
            "HW-2",
            false,
            new DateTimeOffset(2026, 4, 11, 11, 0, 0, TimeSpan.Zero)
        );

        _repositoryMock
            .Setup(repository => repository.GetAllAsync(0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { firstDrain, secondDrain });

        // Act
        IReadOnlyList<DrainResponse> result = await _service.GetAllDrainsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(new DrainResponse(
            firstId,
            firstDrain.Name,
            firstDrain.Address,
            firstDrain.Latitude,
            firstDrain.Longitude,
            firstDrain.IsActive,
            firstDrain.HardwareId,
            firstDrain.CreatedAt
        ));
        result.Should().ContainEquivalentOf(new DrainResponse(
            secondId,
            secondDrain.Name,
            secondDrain.Address,
            secondDrain.Latitude,
            secondDrain.Longitude,
            secondDrain.IsActive,
            secondDrain.HardwareId,
            secondDrain.CreatedAt
        ));

        _repositoryMock.Verify(repository => repository.GetAllAsync(0, 100, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDrainByIdAsync_QuandoIDExiste_DeveRetornarODrainCorreto()
    {
        // Arrange
        Guid drainId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        DrainEntity drain = BuildDrain(
            drainId,
            "Bueiro Principal",
            "Rua Principal",
            12.5,
            45.8,
            "HW-333",
            true,
            new DateTimeOffset(2026, 4, 11, 12, 0, 0, TimeSpan.Zero)
        );

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drain);

        // Act
        DrainResponse result = await _service.GetDrainByIdAsync(drainId);

        // Assert
        result.Should().BeEquivalentTo(new DrainResponse(
            drain.Id,
            drain.Name,
            drain.Address,
            drain.Latitude,
            drain.Longitude,
            drain.IsActive,
            drain.HardwareId,
            drain.CreatedAt
        ));

        _repositoryMock.Verify(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateDrainAsync_ComDadosValidos_DeveCriarERetornarNovoDrain()
    {
        // Arrange
        DrainCreateRequest request = new(
            "Bueiro Novo",
            "Rua Nova",
            11.11,
            22.22,
            "HW-NEW",
            true
        );

        DrainEntity createdDrain = BuildDrain(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.HardwareId,
            request.IsActive,
            new DateTimeOffset(2026, 4, 11, 13, 0, 0, TimeSpan.Zero)
        );

        _repositoryMock
            .Setup(repository => repository.GetByHardwareIdAsync(request.HardwareId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DrainEntity?)null);

        _repositoryMock
            .Setup(repository => repository.CreateAsync(
                It.Is<DrainEntity>(drain =>
                    drain.Name == request.Name
                    && drain.Address == request.Address
                    && drain.Latitude == request.Latitude
                    && drain.Longitude == request.Longitude
                    && drain.HardwareId == request.HardwareId
                    && drain.IsActive == request.IsActive),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDrain);

        // Act
        DrainResponse result = await _service.CreateDrainAsync(request);

        // Assert
        result.Should().BeEquivalentTo(new DrainResponse(
            createdDrain.Id,
            createdDrain.Name,
            createdDrain.Address,
            createdDrain.Latitude,
            createdDrain.Longitude,
            createdDrain.IsActive,
            createdDrain.HardwareId,
            createdDrain.CreatedAt
        ));

        _repositoryMock.Verify(repository => repository.GetByHardwareIdAsync(request.HardwareId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<DrainEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateDrainAsync_ComDadosValidos_DeveAtualizarOsDadosCorretamente()
    {
        // Arrange
        Guid drainId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        DrainEntity existingDrain = BuildDrain(
            drainId,
            "Bueiro Antigo",
            "Rua Antiga",
            5,
            6,
            "HW-OLD",
            true,
            new DateTimeOffset(2026, 4, 11, 14, 0, 0, TimeSpan.Zero)
        );

        DrainUpdateRequest request = new(
            "Bueiro Atualizado",
            "Rua Atualizada",
            7.7,
            8.8,
            false,
            "HW-OLD"
        );

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDrain);

        _repositoryMock
            .Setup(repository => repository.UpdateAsync(
                It.Is<DrainEntity>(drain =>
                    drain.Id == drainId
                    && drain.Name == request.Name
                    && drain.Address == request.Address
                    && drain.Latitude == request.Latitude
                    && drain.Longitude == request.Longitude
                    && drain.IsActive == request.IsActive
                    && drain.HardwareId == request.HardwareId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDrain);

        // Act
        DrainResponse result = await _service.UpdateDrainAsync(drainId, request);

        // Assert
        result.Should().BeEquivalentTo(new DrainResponse(
            existingDrain.Id,
            request.Name!,
            request.Address!,
            request.Latitude!.Value,
            request.Longitude!.Value,
            request.IsActive!.Value,
            request.HardwareId!,
            existingDrain.CreatedAt
        ));

        _repositoryMock.Verify(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<DrainEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateDrainAsync_ComHardwareIdJaEmUso_DeveLancarLogicException()
    {
        // Arrange
        DrainCreateRequest request = new(
            "Bueiro Novo",
            "Rua Nova",
            11.11,
            22.22,
            "HW-JA-USADO",
            true
        );

        _repositoryMock
            .Setup(repository => repository.GetByHardwareIdAsync(request.HardwareId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDrain(
                Guid.Parse("66666666-6666-6666-6666-666666666666"),
                "Outro Bueiro",
                "Rua Outro",
                1,
                2,
                request.HardwareId,
                true,
                DateTimeOffset.UtcNow
            ));

        // Act
        Func<Task> act = () => _service.CreateDrainAsync(request);

        // Assert
        await act.Should().ThrowAsync<LogicException>()
            .WithMessage($"*{request.HardwareId}*");

        _repositoryMock.Verify(repository => repository.GetByHardwareIdAsync(request.HardwareId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.CreateAsync(It.IsAny<DrainEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateDrainAsync_ComDrainInexistente_DeveLancarNotFoundException()
    {
        // Arrange
        Guid drainId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        DrainUpdateRequest request = new("Atualizado", null, null, null, null, null);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DrainEntity?)null);

        // Act
        Func<Task> act = () => _service.UpdateDrainAsync(drainId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{drainId}*");

        _repositoryMock.Verify(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<DrainEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateDrainAsync_ComHardwareIdDeOutroRegistro_DeveLancarLogicException()
    {
        // Arrange
        Guid drainId = Guid.Parse("88888888-8888-8888-8888-888888888888");
        Guid otherDrainId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        DrainEntity existingDrain = BuildDrain(
            drainId,
            "Bueiro Atual",
            "Rua Atual",
            9,
            10,
            "HW-ATUAL",
            true,
            DateTimeOffset.UtcNow
        );

        DrainUpdateRequest request = new(
            null,
            null,
            null,
            null,
            null,
            "HW-OUTRO"
        );

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDrain);

        _repositoryMock
            .Setup(repository => repository.GetByHardwareIdAsync(request.HardwareId!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildDrain(
                otherDrainId,
                "Bueiro Externo",
                "Rua Externa",
                1,
                2,
                request.HardwareId!,
                true,
                DateTimeOffset.UtcNow
            ));

        // Act
        Func<Task> act = () => _service.UpdateDrainAsync(drainId, request);

        // Assert
        await act.Should().ThrowAsync<LogicException>()
            .WithMessage($"*{request.HardwareId}*");

        _repositoryMock.Verify(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.GetByHardwareIdAsync(request.HardwareId!, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<DrainEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteDrainAsync_ComIdInvalido_DeveLancarNotFoundException()
    {
        // Arrange
        Guid drainId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DrainEntity?)null);

        // Act
        Func<Task> act = () => _service.DeleteDrainAsync(drainId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{drainId}*");

        _repositoryMock.Verify(repository => repository.GetByIdAsync(drainId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repository => repository.DeleteAsync(It.IsAny<DrainEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.VerifyNoOtherCalls();
    }

    private static DrainEntity BuildDrain(
        Guid id,
        string name,
        string address,
        double latitude,
        double longitude,
        string hardwareId,
        bool isActive,
        DateTimeOffset createdAt
    )
    {
        return new DrainEntity
        {
            Id = id,
            Name = name,
            Address = address,
            Latitude = latitude,
            Longitude = longitude,
            HardwareId = hardwareId,
            IsActive = isActive,
            CreatedAt = createdAt,
        };
    }
}
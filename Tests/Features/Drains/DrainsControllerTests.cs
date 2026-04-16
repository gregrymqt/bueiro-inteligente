namespace backend.Tests.Features.Drains;

public sealed class DrainsControllerTests
{
    private readonly Mock<IDrainService> _drainServiceMock = new(MockBehavior.Strict);

    private readonly DrainsController _controller;

    public DrainsControllerTests()
    {
        _controller = new DrainsController(_drainServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        IReadOnlyList<DrainResponse> drains =
        [
            new DrainResponse(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Bueiro 1",
                "Rua A",
                10,
                20,
                true,
                "HW-1",
                new DateTimeOffset(2026, 4, 11, 10, 0, 0, TimeSpan.Zero)
            ),
        ];

        _drainServiceMock
            .Setup(service => service.GetAllDrainsAsync(0, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(drains);

        // Act
        ActionResult<IReadOnlyList<DrainResponse>> result = await _controller.GetAll();

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(drains);

        _drainServiceMock.Verify(
            service => service.GetAllDrainsAsync(0, 100, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _drainServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetById_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        Guid drainId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        DrainResponse response = new(
            drainId,
            "Bueiro 2",
            "Rua B",
            30,
            40,
            false,
            "HW-2",
            new DateTimeOffset(2026, 4, 11, 11, 0, 0, TimeSpan.Zero)
        );

        _drainServiceMock
            .Setup(service => service.GetDrainByIdAsync(drainId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ActionResult<DrainResponse> result = await _controller.GetById(drainId);

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);

        _drainServiceMock.Verify(
            service => service.GetDrainByIdAsync(drainId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _drainServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_ComSucesso_DeveRetornarCreatedEApontarParaGetById()
    {
        // Arrange
        DrainCreateRequest request = new("Bueiro Novo", "Rua Nova", 11.11, 22.22, "HW-NEW", true);

        DrainResponse response = new(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.IsActive,
            request.HardwareId,
            new DateTimeOffset(2026, 4, 11, 12, 0, 0, TimeSpan.Zero)
        );

        _drainServiceMock
            .Setup(service => service.CreateDrainAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ActionResult<DrainResponse> result = await _controller.Create(
            request,
            CancellationToken.None
        );

        // Assert
        CreatedAtActionResult createdResult = result
            .Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;
        createdResult.ActionName.Should().Be(nameof(DrainsController.GetById));
        createdResult
            .RouteValues.Should()
            .ContainKey("drainId")
            .WhoseValue.Should()
            .Be(response.Id);
        createdResult.Value.Should().BeEquivalentTo(response);

        _drainServiceMock.Verify(
            service => service.CreateDrainAsync(request, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _drainServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_ComSucesso_DeveRetornarOk()
    {
        // Arrange
        Guid drainId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        DrainUpdateRequest request = new(
            "Atualizado",
            "Rua Atualizada",
            33.33,
            44.44,
            true,
            "HW-UPD"
        );
        DrainResponse response = new(
            drainId,
            request.Name!,
            request.Address!,
            request.Latitude!.Value,
            request.Longitude!.Value,
            request.IsActive!.Value,
            request.HardwareId!,
            new DateTimeOffset(2026, 4, 11, 13, 0, 0, TimeSpan.Zero)
        );

        _drainServiceMock
            .Setup(service =>
                service.UpdateDrainAsync(drainId, request, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(response);

        // Act
        ActionResult<DrainResponse> result = await _controller.Update(
            drainId,
            request,
            CancellationToken.None
        );

        // Assert
        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);

        _drainServiceMock.Verify(
            service => service.UpdateDrainAsync(drainId, request, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _drainServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_ComSucesso_DeveRetornarNoContent()
    {
        // Arrange
        Guid drainId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        _drainServiceMock
            .Setup(service => service.DeleteDrainAsync(drainId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await _controller.Delete(drainId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _drainServiceMock.Verify(
            service => service.DeleteDrainAsync(drainId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _drainServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetById_ComNotFoundException_DeveRetornarNotFound()
    {
        // Arrange
        Guid drainId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        _drainServiceMock
            .Setup(service => service.GetDrainByIdAsync(drainId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Drain", drainId));

        // Act
        ActionResult<DrainResponse> result = await _controller.GetById(drainId);

        // Assert
        ObjectResult objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        ProblemDetails problemDetails = objectResult
            .Value.Should()
            .BeOfType<ProblemDetails>()
            .Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);

        _drainServiceMock.Verify(
            service => service.GetDrainByIdAsync(drainId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _drainServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_ComLogicException_DeveRetornarBadRequest()
    {
        // Arrange
        Guid drainId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        DrainUpdateRequest request = new("Atualizado", null, null, null, null, null);

        _drainServiceMock
            .Setup(service =>
                service.UpdateDrainAsync(drainId, request, It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new LogicException("hardware_id já está em uso."));

        // Act
        ActionResult<DrainResponse> result = await _controller.Update(
            drainId,
            request,
            CancellationToken.None
        );

        // Assert
        ObjectResult objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        ProblemDetails problemDetails = objectResult
            .Value.Should()
            .BeOfType<ProblemDetails>()
            .Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Detail.Should().Contain("hardware_id");

        _drainServiceMock.Verify(
            service => service.UpdateDrainAsync(drainId, request, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _drainServiceMock.VerifyNoOtherCalls();
    }
}

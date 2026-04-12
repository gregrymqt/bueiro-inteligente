using backend.Core;
using backend.Features.Drains.Application.DTOs;
using backend.Features.Drains.Domain.Interfaces;
using DrainEntity = global::backend.Features.Drain.Domain.Drain;

namespace backend.Features.Drains.Application.Services;

public sealed class DrainService(IDrainRepository repository, ILogger<DrainService> logger)
    : IDrainService
{
    private readonly IDrainRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ILogger<DrainService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<DrainResponse>> GetAllDrainsAsync(
        int skip = 0,
        int limit = 100,
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<DrainEntity> drains = await _repository
            .GetAllAsync(skip, limit, cancellationToken)
            .ConfigureAwait(false);

        return drains.Select(MapToResponse).ToList();
    }

    public async Task<DrainResponse> GetDrainByIdAsync(
        Guid drainId,
        CancellationToken cancellationToken = default
    )
    {
        DrainEntity? drain = await _repository
            .GetByIdAsync(drainId, cancellationToken)
            .ConfigureAwait(false);

        if (drain is null)
        {
            throw new NotFoundException("Drain", drainId);
        }

        return MapToResponse(drain);
    }

    public async Task<DrainResponse> CreateDrainAsync(
        DrainCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw LogicException.NullValue(nameof(request));
        }

        ValidateHardwareId(request.HardwareId);

        DrainEntity? existingDrain = await _repository
            .GetByHardwareIdAsync(request.HardwareId, cancellationToken)
            .ConfigureAwait(false);

        if (existingDrain is not null)
        {
            throw new LogicException($"hardware_id '{request.HardwareId}' já está em uso.");
        }

        DrainEntity drain = new()
        {
            Name = request.Name,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            HardwareId = request.HardwareId,
            IsActive = request.IsActive,
        };

        DrainEntity createdDrain = await _repository
            .CreateAsync(drain, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Drain created with id {DrainId}", createdDrain.Id);
        return MapToResponse(createdDrain);
    }

    public async Task<DrainResponse> UpdateDrainAsync(
        Guid drainId,
        DrainUpdateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request is null)
        {
            throw LogicException.NullValue(nameof(request));
        }

        DrainEntity? drain = await _repository
            .GetByIdAsync(drainId, cancellationToken)
            .ConfigureAwait(false);

        if (drain is null)
        {
            throw new NotFoundException("Drain", drainId);
        }

        if (request.Name is not null)
        {
            ValidateTextField(request.Name, nameof(request.Name));
            drain.Name = request.Name;
        }

        if (request.Address is not null)
        {
            ValidateTextField(request.Address, nameof(request.Address));
            drain.Address = request.Address;
        }

        if (request.Latitude.HasValue)
        {
            drain.Latitude = request.Latitude.Value;
        }

        if (request.Longitude.HasValue)
        {
            drain.Longitude = request.Longitude.Value;
        }

        if (request.IsActive.HasValue)
        {
            drain.IsActive = request.IsActive.Value;
        }

        if (request.HardwareId is not null)
        {
            ValidateHardwareId(request.HardwareId);

            if (!string.Equals(request.HardwareId, drain.HardwareId, StringComparison.Ordinal))
            {
                DrainEntity? existingDrain = await _repository
                    .GetByHardwareIdAsync(request.HardwareId, cancellationToken)
                    .ConfigureAwait(false);

                if (existingDrain is not null && existingDrain.Id != drain.Id)
                {
                    throw new LogicException($"hardware_id '{request.HardwareId}' já está em uso.");
                }

                drain.HardwareId = request.HardwareId;
            }
        }

        DrainEntity updatedDrain = await _repository
            .UpdateAsync(drain, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Drain updated with id {DrainId}", updatedDrain.Id);
        return MapToResponse(updatedDrain);
    }

    public async Task DeleteDrainAsync(Guid drainId, CancellationToken cancellationToken = default)
    {
        DrainEntity? drain = await _repository
            .GetByIdAsync(drainId, cancellationToken)
            .ConfigureAwait(false);

        if (drain is null)
        {
            throw new NotFoundException("Drain", drainId);
        }

        await _repository.DeleteAsync(drain, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Drain deleted with id {DrainId}", drainId);
    }

    private static DrainResponse MapToResponse(DrainEntity drain)
    {
        return new DrainResponse(
            drain.Id,
            drain.Name,
            drain.Address,
            drain.Latitude,
            drain.Longitude,
            drain.IsActive,
            drain.HardwareId,
            drain.CreatedAt
        );
    }

    private static void ValidateHardwareId(string hardwareId)
    {
        if (string.IsNullOrWhiteSpace(hardwareId))
        {
            throw LogicException.InvalidValue(nameof(hardwareId), hardwareId);
        }
    }

    private static void ValidateTextField(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw LogicException.InvalidValue(parameterName, value);
        }
    }
}

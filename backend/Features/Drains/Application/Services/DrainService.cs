using backend.Core;
using backend.Features.Drains.Application.DTOs;
using backend.Features.Drains.Domain.Interfaces;
using DrainEntity = global::backend.Features.Drain.Domain.Drain;

namespace backend.Features.Drains.Application.Services;

public sealed class DrainService(IDrainRepository repository, ILogger<DrainService> logger) : IDrainService
{
    private readonly IDrainRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ILogger<DrainService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<DrainResponse>> GetAllDrainsAsync(int skip = 0, int limit = 100, CancellationToken ct = default)
    {
        var drains = await _repository.GetAllAsync(skip, limit, ct).ConfigureAwait(false);
        return [.. drains.Select(MapToResponse)]; // C# 12: Collection expression
    }

    public async Task<DrainResponse> GetDrainByIdAsync(Guid drainId, CancellationToken ct = default)
    {
        var drain = await _repository.GetByIdAsync(drainId, ct).ConfigureAwait(false) 
                    ?? throw new NotFoundException("Drain", drainId);

        return MapToResponse(drain);
    }

    public async Task<DrainResponse> CreateDrainAsync(DrainCreateRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateField(request.HardwareId, nameof(request.HardwareId));

        if (await _repository.GetByHardwareIdAsync(request.HardwareId, ct).ConfigureAwait(false) is not null)
            throw new LogicException($"hardware_id '{request.HardwareId}' já está em uso.");

        DrainEntity drain = new()
        {
            Name = request.Name,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            HardwareId = request.HardwareId,
            IsActive = request.IsActive,
        };

        var created = await _repository.CreateAsync(drain, ct).ConfigureAwait(false);
        _logger.LogInformation("Drain created: {DrainId}", created.Id);
        
        return MapToResponse(created);
    }

    public async Task<DrainResponse> UpdateDrainAsync(Guid drainId, DrainUpdateRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var drain = await _repository.GetByIdAsync(drainId, ct).ConfigureAwait(false) 
                    ?? throw new NotFoundException("Drain", drainId);

        // Atualização de campos simples usando coalescência nula
        drain.Latitude = request.Latitude ?? drain.Latitude;
        drain.Longitude = request.Longitude ?? drain.Longitude;
        drain.IsActive = request.IsActive ?? drain.IsActive;

        // Atualização de strings com validação enxuta
        if (request.Name is not null) drain.Name = ValidateField(request.Name, nameof(request.Name));
        if (request.Address is not null) drain.Address = ValidateField(request.Address, nameof(request.Address));

        if (request.HardwareId is not null && !string.Equals(request.HardwareId, drain.HardwareId, StringComparison.Ordinal))
        {
            ValidateField(request.HardwareId, nameof(request.HardwareId));
            var existing = await _repository.GetByHardwareIdAsync(request.HardwareId, ct).ConfigureAwait(false);
            
            if (existing is not null && existing.Id != drain.Id)
                throw new LogicException($"hardware_id '{request.HardwareId}' já está em uso.");

            drain.HardwareId = request.HardwareId;
        }

        var updated = await _repository.UpdateAsync(drain, ct).ConfigureAwait(false);
        _logger.LogInformation("Drain updated: {DrainId}", updated.Id);
        
        return MapToResponse(updated);
    }

    public async Task DeleteDrainAsync(Guid drainId, CancellationToken ct = default)
    {
        var drain = await _repository.GetByIdAsync(drainId, ct).ConfigureAwait(false) 
                    ?? throw new NotFoundException("Drain", drainId);

        await _repository.DeleteAsync(drain, ct).ConfigureAwait(false);
        _logger.LogInformation("Drain deleted: {DrainId}", drainId);
    }

    private static DrainResponse MapToResponse(DrainEntity d) =>
        new(d.Id, d.Name, d.Address, d.Latitude, d.Longitude, d.IsActive, d.HardwareId, d.CreatedAt);

    private static string ValidateField(string value, string paramName) =>
        !string.IsNullOrWhiteSpace(value) ? value : throw LogicException.InvalidValue(paramName, value);
}
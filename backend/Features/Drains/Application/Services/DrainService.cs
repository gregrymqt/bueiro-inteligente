using backend.Core;
using backend.Features.Drains.Application.DTOs;
using backend.Features.Drains.Application.Interfaces;
using backend.Features.Drains.Domain;
using backend.Features.Drains.Domain.Entities;
using backend.Features.Drains.Domain.Interfaces;
using backend.Features.Monitoring.Domain.Interfaces;
using backend.Features.Monitoring.Application.DTOs;
using backend.Infrastructure.Persistence;

namespace backend.Features.Drains.Application.Services;

public sealed class DrainService(
    IDrainRepository repository,
    IMonitoringRepository monitoringRepository,
    IUnitOfWork unitOfWork,
    ILogger<DrainService> logger
)
    : IDrainService
{
    private readonly IDrainRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IMonitoringRepository _monitoringRepository =
        monitoringRepository ?? throw new ArgumentNullException(nameof(monitoringRepository));
    private readonly IUnitOfWork _unitOfWork =
        unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<DrainService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<DrainResponse>> GetAllDrainsAsync(
        int skip = 0,
        int limit = 100,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation("Listando bueiros. Skip: {Skip}, Limit: {Limit}", skip, limit);

        try
        {
            var drains = await _repository.GetAllAsync(skip, limit, ct).ConfigureAwait(false);
            
            var responses = new List<DrainResponse>(drains.Count);
            foreach (var drain in drains)
            {
                var status = await _monitoringRepository.GetLatestStatusAsync(drain.HardwareId, ct).ConfigureAwait(false);
                responses.Add(MapToResponse(drain, status));
            }
            
            return [.. responses]; // C# 12: Collection expression
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao listar bueiros. Skip: {Skip}, Limit: {Limit}",
                skip,
                limit
            );
            throw;
        }
    }

    public async Task<DrainResponse> GetDrainByIdAsync(Guid drainId, CancellationToken ct = default)
    {
        _logger.LogInformation("Obtendo bueiro {DrainId}.", drainId);

        try
        {
            var drain =
                await _repository.GetByIdAsync(drainId, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("Drain", drainId);

            var status = await _monitoringRepository.GetLatestStatusAsync(drain.HardwareId, ct).ConfigureAwait(false);

            return MapToResponse(drain, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter bueiro {DrainId}.", drainId);
            throw;
        }
    }

    public async Task<DrainResponse> CreateDrainAsync(DrainCreateRequest request, Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Criando bueiro. Request: {@Request}", request);

        try
        {
            ArgumentNullException.ThrowIfNull(request);

            // Validação defensiva para evitar o erro de FK
            if (userId == Guid.Empty)
                throw new LogicException("O ID do usuário é obrigatório para criar um bueiro.");

            var cleanHardwareId = request.HardwareId?.Trim().ToUpperInvariant() ?? string.Empty;

            // Verifica se o hardware já existe (como você já faz)
            if (await _repository.GetByHardwareIdAsync(cleanHardwareId, ct).ConfigureAwait(false) is not null)
                throw new LogicException($"O hardware_id '{cleanHardwareId}' já está em uso.");

            Drain drain = new()
            {
                Name = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                HardwareId = cleanHardwareId,
                IsActive = request.IsActive,
                UserId = userId,
            };

            var created = await _unitOfWork.ExecuteTransactionAsync(async transactionCt =>
            {
                return await _repository.CreateAsync(drain, transactionCt).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
            _logger.LogInformation("Drain created: {DrainId}", created.Id);

            return MapToResponse(created, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bueiro. Request: {@Request}", request);
            throw;
        }
    }

    public async Task<DrainResponse> UpdateDrainAsync(
        Guid drainId,
        DrainUpdateRequest request,
        CancellationToken ct = default
    )
    {
        _logger.LogInformation(
            "Atualizando bueiro {DrainId}. Request: {@Request}",
            drainId,
            request
        );

        try
        {
            ArgumentNullException.ThrowIfNull(request);

            var drain =
                await _repository.GetByIdAsync(drainId, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("Drain", drainId);

            // Atualização de campos simples usando coalescência nula
            drain.Latitude = request.Latitude ?? drain.Latitude;
            drain.Longitude = request.Longitude ?? drain.Longitude;
            drain.IsActive = request.IsActive ?? drain.IsActive;

            // Atualização de strings com validação enxuta
            if (request.Name is not null)
                drain.Name = ValidateField(request.Name, nameof(request.Name));
            if (request.Address is not null)
                drain.Address = ValidateField(request.Address, nameof(request.Address));

            if (
                request.HardwareId is not null
                && !string.Equals(request.HardwareId.Trim(), drain.HardwareId, StringComparison.OrdinalIgnoreCase)
            )
            {
                var cleanHardwareId = request.HardwareId.Trim().ToUpperInvariant();
                ValidateField(cleanHardwareId, nameof(request.HardwareId));
                var existing = await _repository
                    .GetByHardwareIdAsync(cleanHardwareId, ct)
                    .ConfigureAwait(false);

                if (existing is not null && existing.Id != drain.Id)
                    throw new LogicException($"hardware_id '{cleanHardwareId}' já está em uso.");

                drain.HardwareId = cleanHardwareId;
            }

            var updated = await _unitOfWork.ExecuteTransactionAsync(async transactionCt =>
            {
                return await _repository.UpdateAsync(drain, transactionCt).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
            _logger.LogInformation("Drain updated: {DrainId}", updated.Id);

            var status = await _monitoringRepository.GetLatestStatusAsync(updated.HardwareId, ct).ConfigureAwait(false);

            return MapToResponse(updated, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao atualizar bueiro {DrainId}. Request: {@Request}",
                drainId,
                request
            );
            throw;
        }
    }

    public async Task DeleteDrainAsync(Guid drainId, CancellationToken ct = default)
    {
        _logger.LogInformation("Excluindo bueiro {DrainId}.", drainId);

        try
        {
            var drain =
                await _repository.GetByIdAsync(drainId, ct).ConfigureAwait(false)
                ?? throw new NotFoundException("Drain", drainId);

            await _unitOfWork.ExecuteTransactionAsync(async transactionCt =>
            {
                await _repository.DeleteAsync(drain, transactionCt).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);
            _logger.LogInformation("Drain deleted: {DrainId}", drainId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir bueiro {DrainId}.", drainId);
            throw;
        }
    }

    private static DrainResponse MapToResponse(Drain d, DrainStatusDTO? status) =>
        new(
            d.Id,
            d.Name,
            d.Address,
            d.Latitude,
            d.Longitude,
            d.IsActive,
            d.HardwareId,
            d.CreatedAt,
            status?.Status ?? "Normal",
            status?.NivelObstrucao ?? 0.0,
            status?.DistanciaCm ?? d.MaxHeight,
            status?.UltimaAtualizacao ?? DateTimeOffset.UtcNow
        );

    private static string ValidateField(string value, string paramName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value
            : throw LogicException.InvalidValue(paramName, value);
}

using backend.Core;
using backend.Features.Drains.Application.DTOs;
using backend.Features.Drains.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Drains.Presentation.Controllers;

[Authorize(Roles = "User,Admin,Manager")]
public sealed class DrainsController(IDrainService drainService) : ApiControllerBase
{
    // C# 12: Injeção via Primary Constructor
    private readonly IDrainService _drainService = drainService ?? throw new ArgumentNullException(nameof(drainService));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DrainResponse>>> GetAll([FromQuery] int skip = 0, [FromQuery] int limit = 100, CancellationToken ct = default) =>
        await ExecuteAsync(async () => Ok(await _drainService.GetAllDrainsAsync(skip, limit, ct).ConfigureAwait(false)));

    [HttpGet("{drainId:guid}")]
    public async Task<ActionResult<DrainResponse>> GetById(Guid drainId, CancellationToken ct = default) =>
        await ExecuteAsync(async () => Ok(await _drainService.GetDrainByIdAsync(drainId, ct).ConfigureAwait(false)));

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DrainResponse>> Create([FromBody] DrainCreateRequest request, CancellationToken ct = default) =>
        await ExecuteAsync(async () =>
        {
            var result = await _drainService.CreateDrainAsync(request, ct).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { drainId = result.Id }, result);
        });

    [HttpPut("{drainId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DrainResponse>> Update(Guid drainId, [FromBody] DrainUpdateRequest request, CancellationToken ct = default) =>
        await ExecuteAsync(async () => Ok(await _drainService.UpdateDrainAsync(drainId, request, ct).ConfigureAwait(false)));

    [HttpDelete("{drainId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid drainId, CancellationToken ct = default) =>
        await ExecuteAsync(async () =>
        {
            await _drainService.DeleteDrainAsync(drainId, ct).ConfigureAwait(false);
            return NoContent();
        });

    #region Helpers Enxutos

    // Wrapper para centralizar o tratamento de erros e reduzir o boilerplate
    private async Task<ActionResult> ExecuteAsync(Func<Task<ActionResult>> action)
    {
        try { return await action(); }
        catch (NotFoundException ex) { return NotFound(CreateProblem("Not found", ex.Message, 404)); }
        catch (ConnectionException ex) { return StatusCode(503, CreateProblem("Connection error", ex.Message, 503)); }
        catch (LogicException ex) { return BadRequest(CreateProblem("Validation error", ex.Message, 400)); }
    }

    private static ProblemDetails CreateProblem(string title, string detail, int status) => 
        new() { Title = title, Detail = detail, Status = status };

    #endregion
}
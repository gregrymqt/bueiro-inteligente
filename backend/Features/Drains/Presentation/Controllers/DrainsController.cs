using backend.Core;
using backend.Features.Drains.Application.DTOs;
using backend.Features.Drains.Domain.Interfaces;
using backend.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Drains.Presentation.Controllers;

[Authorize(Roles = "User,Admin,Manager")]
public sealed class DrainsController(IDrainService drainService) : ApiControllerBase
{
    private readonly IDrainService _drainService =
        drainService ?? throw new ArgumentNullException(nameof(drainService));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DrainResponse>>> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            IReadOnlyList<DrainResponse> drains = await _drainService
                .GetAllDrainsAsync(skip, limit, cancellationToken)
                .ConfigureAwait(false);

            return Ok(drains);
        }
        catch (NotFoundException exception)
        {
            return CreateProblem(StatusCodes.Status404NotFound, "Not found", exception.Message);
        }
        catch (ConnectionException exception)
        {
            return CreateProblem(
                StatusCodes.Status503ServiceUnavailable,
                "Connection error",
                exception.Message
            );
        }
        catch (LogicException exception)
        {
            return CreateProblem(
                StatusCodes.Status400BadRequest,
                "Validation error",
                exception.Message
            );
        }
    }

    [HttpGet("{drainId:guid}")]
    public async Task<ActionResult<DrainResponse>> GetById(
        Guid drainId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            DrainResponse result = await _drainService
                .GetDrainByIdAsync(drainId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
        catch (NotFoundException exception)
        {
            return CreateProblem(StatusCodes.Status404NotFound, "Not found", exception.Message);
        }
        catch (ConnectionException exception)
        {
            return CreateProblem(
                StatusCodes.Status503ServiceUnavailable,
                "Connection error",
                exception.Message
            );
        }
        catch (LogicException exception)
        {
            return CreateProblem(
                StatusCodes.Status400BadRequest,
                "Validation error",
                exception.Message
            );
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DrainResponse>> Create(
        [FromBody] DrainCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            DrainResponse result = await _drainService
                .CreateDrainAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetById), new { drainId = result.Id }, result);
        }
        catch (NotFoundException exception)
        {
            return CreateProblem(StatusCodes.Status404NotFound, "Not found", exception.Message);
        }
        catch (ConnectionException exception)
        {
            return CreateProblem(
                StatusCodes.Status503ServiceUnavailable,
                "Connection error",
                exception.Message
            );
        }
        catch (LogicException exception)
        {
            return CreateProblem(
                StatusCodes.Status400BadRequest,
                "Validation error",
                exception.Message
            );
        }
    }

    [HttpPut("{drainId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DrainResponse>> Update(
        Guid drainId,
        [FromBody] DrainUpdateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            DrainResponse result = await _drainService
                .UpdateDrainAsync(drainId, request, cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }
        catch (NotFoundException exception)
        {
            return CreateProblem(StatusCodes.Status404NotFound, "Not found", exception.Message);
        }
        catch (ConnectionException exception)
        {
            return CreateProblem(
                StatusCodes.Status503ServiceUnavailable,
                "Connection error",
                exception.Message
            );
        }
        catch (LogicException exception)
        {
            return CreateProblem(
                StatusCodes.Status400BadRequest,
                "Validation error",
                exception.Message
            );
        }
    }

    [HttpDelete("{drainId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        Guid drainId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _drainService.DeleteDrainAsync(drainId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (NotFoundException exception)
        {
            return CreateProblem(StatusCodes.Status404NotFound, "Not found", exception.Message);
        }
        catch (ConnectionException exception)
        {
            return CreateProblem(
                StatusCodes.Status503ServiceUnavailable,
                "Connection error",
                exception.Message
            );
        }
        catch (LogicException exception)
        {
            return CreateProblem(
                StatusCodes.Status400BadRequest,
                "Validation error",
                exception.Message
            );
        }
    }

    private static ObjectResult CreateProblem(int statusCode, string title, string detail)
    {
        return new ObjectResult(
            new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
            }
        )
        {
            StatusCode = statusCode,
        };
    }
}

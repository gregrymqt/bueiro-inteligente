using backend.Core;
using backend.Features.Home.Application.DTOs;
using backend.Features.Home.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Home.Presentation.Controllers;

[ApiController]
[Route("admin/home")]
[Authorize(Roles = "Admin,Manager")]
public sealed class HomeAdminController(IHomeService homeService) : ControllerBase
{
    private readonly IHomeService _homeService =
        homeService ?? throw new ArgumentNullException(nameof(homeService));

    [HttpGet("carousel")]
    public async Task<ActionResult<IReadOnlyList<CarouselResponseDto>>> GetCarousels(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            IReadOnlyList<CarouselResponseDto> carousels = await _homeService
                .GetAllCarouselsAsync(cancellationToken)
                .ConfigureAwait(false);

            return Ok(carousels);
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

    [HttpGet("carousel/{carouselId:guid}")]
    public async Task<ActionResult<CarouselResponseDto>> GetCarouselById(
        Guid carouselId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            CarouselResponseDto result = await _homeService
                .GetCarouselByIdAsync(carouselId, cancellationToken)
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

    [HttpPost("carousel")]
    public async Task<ActionResult<CarouselResponseDto>> CreateCarousel(
        [FromBody] CarouselCreateDto request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            CarouselResponseDto result = await _homeService
                .CreateCarouselAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetCarouselById), new { carouselId = result.Id }, result);
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

    [HttpPatch("carousel/{carouselId:guid}")]
    public async Task<ActionResult<CarouselResponseDto>> UpdateCarousel(
        Guid carouselId,
        [FromBody] CarouselUpdateDto request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            CarouselResponseDto result = await _homeService
                .UpdateCarouselAsync(carouselId, request, cancellationToken)
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

    [HttpDelete("carousel/{carouselId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCarousel(
        Guid carouselId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _homeService.DeleteCarouselAsync(carouselId, cancellationToken)
                .ConfigureAwait(false);

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

    [HttpGet("stats")]
    public async Task<ActionResult<IReadOnlyList<StatCardResponseDto>>> GetStatCards(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            IReadOnlyList<StatCardResponseDto> stats = await _homeService
                .GetAllStatCardsAsync(cancellationToken)
                .ConfigureAwait(false);

            return Ok(stats);
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

    [HttpGet("stats/{statCardId:guid}")]
    public async Task<ActionResult<StatCardResponseDto>> GetStatCardById(
        Guid statCardId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            StatCardResponseDto result = await _homeService
                .GetStatCardByIdAsync(statCardId, cancellationToken)
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

    [HttpPost("stats")]
    public async Task<ActionResult<StatCardResponseDto>> CreateStatCard(
        [FromBody] StatCardCreateDto request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            StatCardResponseDto result = await _homeService
                .CreateStatCardAsync(request, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(nameof(GetStatCardById), new { statCardId = result.Id }, result);
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

    [HttpPatch("stats/{statCardId:guid}")]
    public async Task<ActionResult<StatCardResponseDto>> UpdateStatCard(
        Guid statCardId,
        [FromBody] StatCardUpdateDto request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            StatCardResponseDto result = await _homeService
                .UpdateStatCardAsync(statCardId, request, cancellationToken)
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

    [HttpDelete("stats/{statCardId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStatCard(
        Guid statCardId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _homeService.DeleteStatCardAsync(statCardId, cancellationToken)
                .ConfigureAwait(false);

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
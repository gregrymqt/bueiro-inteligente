using backend.Core;
using backend.Features.Home.Application.DTOs;
using backend.Features.Home.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Home.Presentation.Controllers;

[Authorize(Roles = "Admin,Manager")]
public sealed class HomeAdminController(IHomeService homeService) : ApiControllerBase
{
    private readonly IHomeService _homeService =
        homeService ?? throw new ArgumentNullException(nameof(homeService));

    #region Carousel Endpoints

    [HttpGet("carousel")]
    public async Task<ActionResult<IReadOnlyList<CarouselResponseDto>>> GetCarousels(
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
            Ok(await _homeService.GetAllCarouselsAsync(ct).ConfigureAwait(false))
        );

    [HttpGet("carousel/{carouselId:guid}")]
    public async Task<ActionResult<CarouselResponseDto>> GetCarouselById(
        Guid carouselId,
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
            Ok(await _homeService.GetCarouselByIdAsync(carouselId, ct).ConfigureAwait(false))
        );

    [HttpPost("carousel")]
    public async Task<ActionResult<CarouselResponseDto>> CreateCarousel(
        [FromBody] CarouselCreateDto request,
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
        {
            var result = await _homeService.CreateCarouselAsync(request, ct).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetCarouselById), new { carouselId = result.Id }, result);
        });

    [HttpPatch("carousel/{carouselId:guid}")]
    public async Task<ActionResult<CarouselResponseDto>> UpdateCarousel(
        Guid carouselId,
        [FromBody] CarouselUpdateDto request,
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
            Ok(
                await _homeService
                    .UpdateCarouselAsync(carouselId, request, ct)
                    .ConfigureAwait(false)
            )
        );

    [HttpDelete("carousel/{carouselId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCarousel(Guid carouselId, CancellationToken ct) =>
        await ExecuteAsync(async () =>
        {
            await _homeService.DeleteCarouselAsync(carouselId, ct).ConfigureAwait(false);
            return NoContent();
        });

    #endregion

    #region Stats Endpoints

    [HttpGet("stats")]
    public async Task<ActionResult<IReadOnlyList<StatCardResponseDto>>> GetStatCards(
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
            Ok(await _homeService.GetAllStatCardsAsync(ct).ConfigureAwait(false))
        );

    [HttpGet("stats/{statCardId:guid}")]
    public async Task<ActionResult<StatCardResponseDto>> GetStatCardById(
        Guid statCardId,
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
            Ok(await _homeService.GetStatCardByIdAsync(statCardId, ct).ConfigureAwait(false))
        );

    [HttpPost("stats")]
    public async Task<ActionResult<StatCardResponseDto>> CreateStatCard(
        [FromBody] StatCardCreateDto request,
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
        {
            var result = await _homeService.CreateStatCardAsync(request, ct).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetStatCardById), new { statCardId = result.Id }, result);
        });

    [HttpPatch("stats/{statCardId:guid}")]
    public async Task<ActionResult<StatCardResponseDto>> UpdateStatCard(
        Guid statCardId,
        [FromBody] StatCardUpdateDto request,
        CancellationToken ct
    ) =>
        await ExecuteAsync(async () =>
            Ok(
                await _homeService
                    .UpdateStatCardAsync(statCardId, request, ct)
                    .ConfigureAwait(false)
            )
        );

    [HttpDelete("stats/{statCardId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStatCard(Guid statCardId, CancellationToken ct) =>
        await ExecuteAsync(async () =>
        {
            await _homeService.DeleteStatCardAsync(statCardId, ct).ConfigureAwait(false);
            return NoContent();
        });

    #endregion

    #region Helpers Enxutos

    private async Task<ActionResult> ExecuteAsync(Func<Task<ActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (NotFoundException ex)
        {
            return NotFound(CreateProblem("Not found", ex.Message, 404));
        }
        catch (ConnectionException ex)
        {
            return StatusCode(503, CreateProblem("Connection error", ex.Message, 503));
        }
        catch (LogicException ex)
        {
            return BadRequest(CreateProblem("Validation error", ex.Message, 400));
        }
    }

    private static ProblemDetails CreateProblem(string title, string detail, int status) =>
        new()
        {
            Title = title,
            Detail = detail,
            Status = status,
        };

    #endregion
}

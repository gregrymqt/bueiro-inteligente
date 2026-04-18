using backend.Core;
using backend.Features.Home.Application.DTOs;
using backend.Features.Home.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Home.Presentation.Controllers;

public sealed class HomeController(IHomeService homeService) : ApiControllerBase
{
    private readonly IHomeService _homeService =
        homeService ?? throw new ArgumentNullException(nameof(homeService));

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<HomeResponseDto>> GetHomeContent(CancellationToken ct) =>
        await ExecuteAsync(async () =>
            Ok(await _homeService.GetHomeContentAsync(ct).ConfigureAwait(false))
        );

    #region Helpers Enxutos

    private async Task<ActionResult> ExecuteAsync(Func<Task<ActionResult>> action)
    {
        try
        {
            return await action();
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

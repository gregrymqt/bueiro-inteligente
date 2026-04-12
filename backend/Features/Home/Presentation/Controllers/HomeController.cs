using backend.Core;
using backend.Features.Home.Application.DTOs;
using backend.Features.Home.Application.Interfaces;
using backend.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Home.Presentation.Controllers;

public sealed class HomeController(IHomeService homeService) : ApiControllerBase
{
    private readonly IHomeService _homeService =
        homeService ?? throw new ArgumentNullException(nameof(homeService));

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<HomeResponseDto>> GetHomeContent(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            HomeResponseDto result = await _homeService
                .GetHomeContentAsync(cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
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
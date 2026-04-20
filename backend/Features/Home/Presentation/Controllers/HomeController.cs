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
        Ok(await _homeService.GetHomeContentAsync(ct).ConfigureAwait(false));
}

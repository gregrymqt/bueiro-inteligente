using backend.Infrastructure.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
[ServiceFilter(typeof(RateLimitFilter))]
public abstract class ApiControllerBase : ControllerBase { }

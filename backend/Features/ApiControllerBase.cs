using backend.Infrastructure.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize] // Garante o JWT em tudo que herdar daqui
[ServiceFilter(typeof(RateLimitFilter))]
public abstract class ApiControllerBase : ControllerBase
{
    // Métodos auxiliares protegidos podem entrar aqui para evitar repetição nas controllers
}

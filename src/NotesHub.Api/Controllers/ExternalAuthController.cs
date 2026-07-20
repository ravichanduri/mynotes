using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace NotesHub.Api.Controllers;
[ApiController, Route("api/auth/external")]
public sealed class ExternalAuthController : ControllerBase
{
    [HttpGet("{provider}")] public IActionResult ChallengeProvider(string provider, [FromQuery] string returnUrl = "/") => provider is "Google" or "GitHub" ? Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, provider) : NotFound();
}

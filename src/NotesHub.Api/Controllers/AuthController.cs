using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotesHub.Application;
using NotesHub.Infrastructure;

namespace NotesHub.Api.Controllers;
[ApiController, Route("api/auth")]
public sealed class AuthController(UserManager<ApplicationUser> users, ITokenService tokens, IValidator<RegisterRequest> registerValidator, ICurrentUser current) : ControllerBase
{
    [HttpPost("register")] public async Task<ActionResult<TokenResponse>> Register(RegisterRequest request, CancellationToken ct) { await registerValidator.ValidateAndThrowAsync(request, ct); var user = new ApplicationUser { UserName = request.Email, Email = request.Email, DisplayName = request.DisplayName }; var result = await users.CreateAsync(user, request.Password); if (!result.Succeeded) return BadRequest(new { errors = result.Errors }); return Ok(await tokens.CreateAsync(user.Id, user.Email!, [], ct)); }
    [HttpPost("login")] public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken ct) { var user = await users.FindByEmailAsync(request.Email); if (user is null || !user.IsActive || !await users.CheckPasswordAsync(user, request.Password)) return Unauthorized(); return Ok(await tokens.CreateAsync(user.Id, user.Email!, await users.GetRolesAsync(user), ct)); }
    [HttpPost("refresh")] public async Task<ActionResult<TokenResponse>> Refresh([FromBody] string refreshToken, CancellationToken ct) => (await tokens.RefreshAsync(refreshToken, ct)) is { } response ? Ok(response) : Unauthorized();
    [Authorize, HttpPost("logout")] public async Task<IActionResult> Logout([FromBody] string refreshToken, CancellationToken ct) { await tokens.RevokeAsync(refreshToken, ct); return NoContent(); }
    [Authorize, HttpGet("profile")] public async Task<ActionResult<ProfileDto>> Profile() { var user = await users.FindByIdAsync(current.Id) ?? throw new UnauthorizedAccessException(); return Ok(new ProfileDto(user.Id, user.Email!, user.DisplayName, user.Tier, user.IsActive, (await users.GetRolesAsync(user)).ToArray())); }
}

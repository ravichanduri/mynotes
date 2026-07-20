using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesHub.Application;
using NotesHub.Infrastructure;

namespace NotesHub.Api.Controllers;
[ApiController, Authorize(Policy = "Admin"), Route("api/admin")]
public sealed class AdminController(UserManager<ApplicationUser> users) : ControllerBase
{
    [HttpGet("users")] public async Task<IReadOnlyList<ProfileDto>> Users(CancellationToken ct) => await users.Users.OrderBy(x => x.Email).Select(x => new ProfileDto(x.Id, x.Email!, x.DisplayName, x.Tier, x.IsActive, Array.Empty<string>())).ToListAsync(ct);
    [HttpPut("users/{id}/activate")] public Task<IActionResult> Activate(string id) => SetActive(id, true);
    [HttpPut("users/{id}/deactivate")] public Task<IActionResult> Deactivate(string id) => SetActive(id, false);
    [HttpPut("users/{id}/tier")] public async Task<IActionResult> Tier(string id, ChangeTierRequest request) { var user = await users.FindByIdAsync(id); if (user is null) return NotFound(); user.Tier = request.Tier; return (await users.UpdateAsync(user)).Succeeded ? NoContent() : BadRequest(); }
    [HttpDelete("users/{id}")] public async Task<IActionResult> Delete(string id) { var user = await users.FindByIdAsync(id); if (user is null) return NotFound(); return (await users.DeleteAsync(user)).Succeeded ? NoContent() : BadRequest(); }
    private async Task<IActionResult> SetActive(string id, bool active) { var user = await users.FindByIdAsync(id); if (user is null) return NotFound(); user.IsActive = active; return (await users.UpdateAsync(user)).Succeeded ? NoContent() : BadRequest(); }
}

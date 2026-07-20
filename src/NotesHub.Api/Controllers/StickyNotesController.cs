using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesHub.Application;
using NotesHub.Domain;

namespace NotesHub.Api.Controllers;
[ApiController, Authorize, Route("api/sticky-notes")]
public sealed class StickyNotesController(IStickyNoteRepository notes, ICurrentUser user, IValidator<StickyNoteRequest> validator) : ControllerBase
{
    [HttpGet] public async Task<IReadOnlyList<StickyNoteDto>> List(CancellationToken ct) => (await notes.ListAsync(user.Id, ct)).Select(Map).ToList();
    [HttpPost] public async Task<ActionResult<StickyNoteDto>> Create(StickyNoteRequest request, CancellationToken ct) { await validator.ValidateAndThrowAsync(request, ct); var note = new StickyNote { OwnerId = user.Id }; note.Update(request.Content, request.Color, request.PositionX, request.PositionY); await notes.AddAsync(note, ct); await notes.SaveAsync(ct); return Ok(Map(note)); }
    [HttpPut("{id:guid}")] public async Task<ActionResult<StickyNoteDto>> Update(Guid id, StickyNoteRequest request, CancellationToken ct) { await validator.ValidateAndThrowAsync(request, ct); var note = await notes.GetAsync(id, user.Id, ct); if (note is null) return NotFound(); note.Update(request.Content, request.Color, request.PositionX, request.PositionY); await notes.SaveAsync(ct); return Ok(Map(note)); }
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { var note = await notes.GetAsync(id, user.Id, ct); if (note is null) return NotFound(); notes.Remove(note); await notes.SaveAsync(ct); return NoContent(); }
    private static StickyNoteDto Map(StickyNote n) => new(n.Id, n.Content, n.Color, n.PositionX, n.PositionY, n.CreatedAt);
}

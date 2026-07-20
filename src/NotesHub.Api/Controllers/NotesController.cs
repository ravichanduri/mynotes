using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesHub.Application;
using NotesHub.Domain;

namespace NotesHub.Api.Controllers;
[ApiController, Authorize, Route("api/notes")]
public sealed class NotesController(INoteRepository notes, ICurrentUser user, IValidator<NoteRequest> validator) : ControllerBase
{
    [HttpGet] public async Task<IReadOnlyList<NoteDto>> List([FromQuery] string? q, CancellationToken ct) => (await notes.SearchAsync(user.Id, q, ct)).Select(Map).ToList();
    [HttpPost] public async Task<ActionResult<NoteDto>> Create(NoteRequest request, CancellationToken ct) { await validator.ValidateAndThrowAsync(request, ct); var note = new Note { OwnerId = user.Id }; note.Update(request.Title, request.Content, request.IsFavorite); await notes.AddAsync(note, ct); await notes.SaveAsync(ct); return CreatedAtAction(nameof(Get), new { id = note.Id }, Map(note)); }
    [HttpGet("{id:guid}")] public async Task<ActionResult<NoteDto>> Get(Guid id, CancellationToken ct) => await notes.GetAsync(id, user.Id, ct) is { } n ? Ok(Map(n)) : NotFound();
    [HttpPut("{id:guid}")] public async Task<ActionResult<NoteDto>> Update(Guid id, NoteRequest request, CancellationToken ct) { await validator.ValidateAndThrowAsync(request, ct); var note = await notes.GetAsync(id, user.Id, ct); if (note is null) return NotFound(); note.Update(request.Title, request.Content, request.IsFavorite); await notes.SaveAsync(ct); return Ok(Map(note)); }
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { var note = await notes.GetAsync(id, user.Id, ct); if (note is null) return NotFound(); notes.Remove(note); await notes.SaveAsync(ct); return NoContent(); }
    private static NoteDto Map(Note n) => new(n.Id, n.Title, n.Content, n.IsFavorite, n.CreatedAt, n.UpdatedAt);
}

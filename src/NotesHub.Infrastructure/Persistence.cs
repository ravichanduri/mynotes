using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotesHub.Application;
using NotesHub.Domain;

namespace NotesHub.Infrastructure;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public UserTier Tier { get; set; } = UserTier.Free;
    public bool IsActive { get; set; } = true;
}

public sealed class NotesHubDbContext(DbContextOptions<NotesHubDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Note> Notes => Set<Note>(); public DbSet<StickyNote> StickyNotes => Set<StickyNote>(); public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Note>(e => { e.ToTable("Notes"); e.HasKey(x => x.Id); e.Property(x => x.Title).HasMaxLength(200).IsRequired(); e.HasIndex(x => new { x.OwnerId, x.CreatedAt }); });
        b.Entity<StickyNote>(e => { e.ToTable("StickyNotes"); e.HasKey(x => x.Id); e.Property(x => x.Color).HasMaxLength(7).IsRequired(); e.HasIndex(x => x.OwnerId); });
        b.Entity<RefreshToken>(e => { e.ToTable("RefreshTokens"); e.HasKey(x => x.Id); e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired(); e.HasIndex(x => x.TokenHash).IsUnique(); });
    }
}

public sealed class NoteRepository(NotesHubDbContext db) : INoteRepository
{
    public Task<IReadOnlyList<Note>> SearchAsync(string ownerId, string? query, CancellationToken ct) => db.Notes.Where(x => x.OwnerId == ownerId && (string.IsNullOrWhiteSpace(query) || x.Title.Contains(query) || x.Content.Contains(query))).OrderByDescending(x => x.IsFavorite).ThenByDescending(x => x.UpdatedAt ?? x.CreatedAt).ToListAsync(ct).ContinueWith(t => (IReadOnlyList<Note>)t.Result, ct);
    public Task<Note?> GetAsync(Guid id, string ownerId, CancellationToken ct) => db.Notes.SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, ct);
    public Task AddAsync(Note note, CancellationToken ct) => db.Notes.AddAsync(note, ct).AsTask(); public void Remove(Note note) => db.Notes.Remove(note); public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
public sealed class StickyNoteRepository(NotesHubDbContext db) : IStickyNoteRepository
{
    public Task<IReadOnlyList<StickyNote>> ListAsync(string ownerId, CancellationToken ct) => db.StickyNotes.Where(x => x.OwnerId == ownerId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct).ContinueWith(t => (IReadOnlyList<StickyNote>)t.Result, ct);
    public Task<StickyNote?> GetAsync(Guid id, string ownerId, CancellationToken ct) => db.StickyNotes.SingleOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, ct);
    public Task AddAsync(StickyNote note, CancellationToken ct) => db.StickyNotes.AddAsync(note, ct).AsTask(); public void Remove(StickyNote note) => db.StickyNotes.Remove(note); public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

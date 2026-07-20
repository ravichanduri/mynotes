using FluentValidation;
using NotesHub.Domain;

namespace NotesHub.Application;

public record NoteRequest(string Title, string Content, bool IsFavorite);
public record NoteDto(Guid Id, string Title, string Content, bool IsFavorite, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);
public record StickyNoteRequest(string Content, string Color, double PositionX, double PositionY);
public record StickyNoteDto(Guid Id, string Content, string Color, double PositionX, double PositionY, DateTimeOffset CreatedAt);
public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record TokenResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
public record ProfileDto(string Id, string Email, string DisplayName, UserTier Tier, bool IsActive, IReadOnlyCollection<string> Roles);
public record ChangeTierRequest(UserTier Tier);

public interface INoteRepository { Task<IReadOnlyList<Note>> SearchAsync(string ownerId, string? query, CancellationToken ct); Task<Note?> GetAsync(Guid id, string ownerId, CancellationToken ct); Task AddAsync(Note note, CancellationToken ct); void Remove(Note note); Task SaveAsync(CancellationToken ct); }
public interface IStickyNoteRepository { Task<IReadOnlyList<StickyNote>> ListAsync(string ownerId, CancellationToken ct); Task<StickyNote?> GetAsync(Guid id, string ownerId, CancellationToken ct); Task AddAsync(StickyNote note, CancellationToken ct); void Remove(StickyNote note); Task SaveAsync(CancellationToken ct); }
public interface ICurrentUser { string Id { get; } bool IsAuthenticated { get; } }
public interface ITokenService { Task<TokenResponse> CreateAsync(string userId, string email, IEnumerable<string> roles, CancellationToken ct); Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken ct); Task RevokeAsync(string refreshToken, CancellationToken ct); }

public sealed class NoteRequestValidator : AbstractValidator<NoteRequest> { public NoteRequestValidator() { RuleFor(x => x.Title).NotEmpty().MaximumLength(200); RuleFor(x => x.Content).MaximumLength(10000); } }
public sealed class StickyNoteRequestValidator : AbstractValidator<StickyNoteRequest> { public StickyNoteRequestValidator() { RuleFor(x => x.Content).MaximumLength(2000); RuleFor(x => x.Color).Matches("^#[0-9a-fA-F]{6}$"); RuleFor(x => x.PositionX).GreaterThanOrEqualTo(0); RuleFor(x => x.PositionY).GreaterThanOrEqualTo(0); } }
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest> { public RegisterRequestValidator() { RuleFor(x => x.Email).EmailAddress(); RuleFor(x => x.Password).MinimumLength(12); RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100); } }

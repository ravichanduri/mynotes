using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NotesHub.Application;
using NotesHub.Domain;

namespace NotesHub.Infrastructure;
public sealed class JwtTokenService(NotesHubDbContext db, IConfiguration config) : ITokenService
{
    public async Task<TokenResponse> CreateAsync(string userId, string email, IEnumerable<string> roles, CancellationToken ct)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(15); var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId), new(JwtRegisteredClaimNames.Email, email), new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) }; claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));
        var jwt = new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Audience"], claims, expires: expires.UtcDateTime, signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)); db.RefreshTokens.Add(new RefreshToken { UserId = userId, TokenHash = Hash(raw), ExpiresAt = DateTimeOffset.UtcNow.AddDays(30) }); await db.SaveChangesAsync(ct);
        return new(new JwtSecurityTokenHandler().WriteToken(jwt), raw, expires);
    }
    public async Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken ct)
    { var token = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == Hash(refreshToken), ct); if (token is null || !token.IsActive) return null; token.Revoke(); var user = await db.Users.FindAsync([token.UserId], ct); if (user is null || !user.IsActive) return null; await db.SaveChangesAsync(ct); return await CreateAsync(user.Id, user.Email!, Array.Empty<string>(), ct); }
    public async Task RevokeAsync(string refreshToken, CancellationToken ct) { var token = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == Hash(refreshToken), ct); if (token?.IsActive == true) { token.Revoke(); await db.SaveChangesAsync(ct); } }
    private static string Hash(string v) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(v)));
}

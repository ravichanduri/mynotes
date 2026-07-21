using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotesHub.Application;
using NotesHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<NotesHubDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddIdentityCore<ApplicationUser>(o => { o.Password.RequiredLength = 12; o.User.RequireUniqueEmail = true; o.SignIn.RequireConfirmedEmail = false; })
    .AddRoles<IdentityRole>().AddEntityFrameworkStores<NotesHubDbContext>().AddDefaultTokenProviders();
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");
var auth = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => o.TokenValidationParameters = new()
{ ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), NameClaimType = ClaimTypes.NameIdentifier, RoleClaimType = ClaimTypes.Role });
if (!string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Google:ClientId"])) auth.AddGoogle("Google", o => { o.ClientId = builder.Configuration["Authentication:Google:ClientId"]!; o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!; });
if (!string.IsNullOrWhiteSpace(builder.Configuration["Authentication:GitHub:ClientId"])) auth.AddGitHub("GitHub", o => { o.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!; o.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!; });
builder.Services.AddAuthorization(o => o.AddPolicy("Admin", p => p.RequireRole("Admin")));
builder.Services.AddScoped<INoteRepository, NoteRepository>(); builder.Services.AddScoped<IStickyNoteRepository, StickyNoteRepository>(); builder.Services.AddScoped<ITokenService, JwtTokenService>(); builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddValidatorsFromAssemblyContaining<NoteRequestValidator>();
builder.Services.AddCors(o => o.AddPolicy("web", p => p.WithOrigins(builder.Configuration["Cors:Origin"] ?? "http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>(); app.UseSwagger(); app.UseSwaggerUI(); app.UseHttpsRedirection(); app.UseCors("web"); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
using (var scope = app.Services.CreateScope()) { var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(); if (!await roles.RoleExistsAsync("Admin")) await roles.CreateAsync(new IdentityRole("Admin")); }
app.Run();
public partial class Program { }

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{ public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true; public string Id => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException(); }
public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{ public async Task Invoke(HttpContext context) { try { await next(context); } catch (ValidationException ex) { context.Response.StatusCode = 400; await context.Response.WriteAsJsonAsync(new { error = "Validation failed", details = ex.Errors.Select(x => new { x.PropertyName, x.ErrorMessage }) }); } catch (UnauthorizedAccessException) { context.Response.StatusCode = 401; } catch (ArgumentException ex) { context.Response.StatusCode = 400; await context.Response.WriteAsJsonAsync(new { error = ex.Message }); } catch (Exception ex) { logger.LogError(ex, "Unhandled exception"); context.Response.StatusCode = 500; await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." }); } } }

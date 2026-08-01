using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using LibraryManagementApi.Api.Endpoints;
using LibraryManagementApi.Api.Middleware;
using LibraryManagementApi.Application;
using LibraryManagementApi.Infrastructure;
using LibraryManagementApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Without this, System.Text.Json's default enum handling serializes MembershipStatus,
// LoanStatus, and ReservationStatus as raw integers in every DTO that carries one — invisible
// from C#-to-C# testing (both sides agree on the numeric value), but a real API consumer
// (the frontend) has no way to know 0 means "Active" without this.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Reuses ClientApp:BaseUrl (already the canonical "where the frontend lives" setting used for
// password-reset links) rather than introducing a second place to configure the same value.
var clientAppBaseUrl = builder.Configuration["ClientApp:BaseUrl"]
    ?? throw new InvalidOperationException("ClientApp:BaseUrl configuration is missing.");
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins(clientAppBaseUrl)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Auth endpoints (login, register, forgot-password) are the classic brute-force/credential-
// stuffing/enumeration targets — a per-client-IP window limits how fast an attacker can try
// passwords or probe for registered emails without affecting normal usage. Configurable rather
// than hardcoded specifically so appsettings.Testing.json can raise the ceiling — the
// integration test suite's HttpClient runs entirely through WebApplicationFactory's in-memory
// TestServer, where every request shares one connection/partition key, so the ~100 auth calls
// across the full suite would otherwise trip a production-strength limit within seconds.
var authRateLimit = builder.Configuration.GetSection("RateLimiting:Auth").Get<RateLimitOptions>()
    ?? new RateLimitOptions();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authRateLimit.PermitLimit,
            Window = TimeSpan.FromSeconds(authRateLimit.WindowSeconds),
            QueueLimit = 0,
        }));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedRolesAsync(scope.ServiceProvider);

    if (app.Environment.IsDevelopment())
    {
        await DbInitializer.SeedDevAccountsAsync(scope.ServiceProvider, app.Configuration);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // interactive API docs at /scalar/v1 — see README's "API Documentation" section
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    await next();
});

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapBranchEndpoints();
app.MapBookEndpoints();
app.MapMemberEndpoints();
app.MapLoanEndpoints();
app.MapReservationEndpoints();
app.MapReportEndpoints();

app.Run();

public partial class Program
{
}

public class RateLimitOptions
{
    public int PermitLimit { get; set; } = 10;

    public int WindowSeconds { get; set; } = 60;
}

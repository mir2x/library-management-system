using LibraryManagementApi.Api.Endpoints;
using LibraryManagementApi.Api.Middleware;
using LibraryManagementApi.Application;
using LibraryManagementApi.Infrastructure;
using LibraryManagementApi.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedRolesAsync(scope.ServiceProvider);

    if (app.Environment.IsDevelopment())
    {
        await DbInitializer.SeedDefaultAdminAsync(scope.ServiceProvider, app.Configuration);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.UseCors("Frontend");

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

using System.Text;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Infrastructure.Identity;
using LibraryManagementApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagementApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing. Set Jwt:Secret via user-secrets and Jwt:Issuer/Jwt:Audience in appsettings.json.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim types exactly as issued (e.g. "sub") instead of ASP.NET Core's
                // default remap to long-form ClaimTypes.* URIs, so claim lookups match what
                // JwtTokenGenerator actually wrote into the token.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}

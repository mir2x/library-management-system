using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: register ApplicationDbContext (Npgsql), Identity/JWT services, and other
        // Infrastructure implementations here as features are added. See ARCHITECTURE.md.

        return services;
    }
}

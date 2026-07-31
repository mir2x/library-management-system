using FluentValidation;
using LibraryManagementApi.Application.Common.Behaviours;
using LibraryManagementApi.Application.Common.Interfaces;
using LibraryManagementApi.Application.Common.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();

        return services;
    }
}

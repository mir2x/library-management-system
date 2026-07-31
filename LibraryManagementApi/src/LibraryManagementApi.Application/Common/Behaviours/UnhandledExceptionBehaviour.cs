using LibraryManagementApi.Application.Common.Exceptions;
using LibraryManagementApi.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using ValidationException = LibraryManagementApi.Application.Common.Exceptions.ValidationException;

namespace LibraryManagementApi.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<UnhandledExceptionBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex) when (ex is not ValidationException and not NotFoundException and not DomainException)
        {
            logger.LogError(ex, "Unhandled exception for request {RequestName} {@Request}", typeof(TRequest).Name, request);
            throw;
        }
    }
}

using MediatR;
using Microsoft.Extensions.Logging;

namespace Framework.Application.Common.Behaviours;

public sealed class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for request {RequestName} {@Request}", typeof(TRequest).Name, request);
            throw;
        }
    }
}

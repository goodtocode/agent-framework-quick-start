using System.ClientModel;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Goodtocode.AgentFramework.Core.Application.Common.Behaviors;

public class CustomUnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<TRequest> logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestDelegateInvoker<TResponse> nextInvoker, CancellationToken cancellationToken)
    {
        try
        {
            return await nextInvoker();
        }
        catch (ClientResultException ex)
        {
            var requestName = typeof(TRequest).Name;
            var clientResultDetails = ClientResultExceptionLogHelper.FormatProperties(ex);
            await Task.Run(() => logger.LogError(ex,
                "Request: ClientResultException for {Name}. Details: {Details}",
                requestName,
                clientResultDetails), cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            await Task.Run(() => logger.LogUnhandledException(ex, requestName), cancellationToken);
            throw;
        }
    }
}

public class CustomUnhandledExceptionBehavior<TRequest>(ILogger<TRequest> logger) : IPipelineBehavior<TRequest> where TRequest : notnull
{
    private readonly ILogger<TRequest> logger = logger;

    public async Task Handle(TRequest request, RequestDelegateInvoker nextInvoker, CancellationToken cancellationToken)
    {
        try
        {
            await nextInvoker();
        }
        catch (ClientResultException ex)
        {
            var requestName = typeof(TRequest).Name;
            var clientResultDetails = ClientResultExceptionLogHelper.FormatProperties(ex);
            await Task.Run(() => logger.LogError(ex,
                "Request: ClientResultException for {Name}. Details: {Details}",
                requestName,
                clientResultDetails), cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            await Task.Run(() => logger.LogUnhandledException(ex, requestName), cancellationToken);
            throw;
        }
    }
}

internal static class ClientResultExceptionLogHelper
{
    public static string FormatProperties(ClientResultException exception)
    {
        var properties = exception.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Select(property =>
            {
                try
                {
                    var value = property.GetValue(exception);
                    return value switch
                    {
                        null => $"{property.Name}=<null>",
                        string text => $"{property.Name}={text}",
                        _ => $"{property.Name}={value}"
                    };
                }
                catch (Exception readException)
                {
                    return $"{property.Name}=<unavailable:{readException.GetType().Name}>";
                }
            });

        return string.Join("; ", properties);
    }
}

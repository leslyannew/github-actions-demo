using MediatR;

namespace github_actions_demo.Behaviors;
internal sealed class LoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)

    {
        string requestName = typeof(TRequest).Name;
        try
        {
            logger.LogInformation("Executing request {RequestName}", requestName);

            TResponse response = await next();

            logger.LogInformation("Request {RequestName} processed successfully", requestName);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request {RequestName} processing failed", requestName);

            throw;
        }
    }
}

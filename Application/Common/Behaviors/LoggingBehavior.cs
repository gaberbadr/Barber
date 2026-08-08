using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private const int SlowRequestThresholdMs = 3000;

        public LoggingBehavior(
            ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            var traceId = Activity.Current?.TraceId.ToString()
                          ?? Guid.NewGuid().ToString();

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = traceId,
                ["RequestName"] = requestName
            }))
            {
                _logger.LogInformation(
                    "Processing request {RequestName}",
                    requestName);

                try
                {
                    var response = await next();
                    stopwatch.Stop();
                    var elapsed = stopwatch.ElapsedMilliseconds;

                    if (elapsed > SlowRequestThresholdMs)
                    {
                        _logger.LogWarning(
                            "Slow request {RequestName} took {ElapsedMs}ms",
                            requestName,
                            elapsed);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Completed request {RequestName} in {ElapsedMs}ms",
                            requestName,
                            elapsed);
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(
                        ex,
                        "Request {RequestName} failed after {ElapsedMs}ms",
                        requestName,
                        stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }
        }
    }
}

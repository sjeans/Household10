using MediatR;
using System.Diagnostics;
using System.Text.Json;
using Serilog;

namespace Household.Application.Common.Behaviors;

public class LoggingPipelineBehavior<TRequest, TResponse>(ILogger logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger _logger = logger.ForContext<LoggingPipelineBehavior<TRequest, TResponse>>();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = request.GetType().Name;
        string requestGuid = request.GetType().GUID.ToString();
        string requestNameWithGuid = $"{requestName} [{requestGuid}]";

        _logger.Information("[START] {requestNameWithGuid}", requestNameWithGuid);

        TResponse response;
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            try
            {
                _logger.Information("[PROPS] {name} {properties}", requestNameWithGuid, JsonSerializer.Serialize(request));
            }
            catch (NotSupportedException)
            {
                _logger.Information("[Serialization ERROR] {name} Could not serialize the request.", requestNameWithGuid);
            }

            response = await next();
        }
        finally
        {
            stopwatch.Stop();
            _logger.Information("[END] {name}; Execution time={time}ms", requestNameWithGuid, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
namespace FiapCloudGames.AzureFunctions.Functions.Middlewares;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Serilog;
using Serilog.Context;

public class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        HttpRequestData? httpRequest = await context.GetHttpRequestDataAsync();
        string? correlationId = null;

        if (httpRequest is not null && httpRequest.Headers.TryGetValues("X-Correlation-ID", out IEnumerable<string>? values))
        {
            correlationId = values.FirstOrDefault();
            HttpResponseData httpResponseData = context.GetHttpResponseData()!;
            httpResponseData.Headers.Add("X-Correlation-ID", correlationId);
        }
        else
            correlationId = Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            Log.Information("CorrelationId {correlationId}", correlationId);
            context.Items.Add("CorrelationId", correlationId!);
            await next(context);
        }
    }
}

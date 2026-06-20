using System.Diagnostics.CodeAnalysis;
using Serilog.Context;

namespace Fcs.Campaign.WebApi.Middlewares;

[ExcludeFromCodeCoverage]
public sealed class GlobalCorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private const string CorrelationIdKey = "CorrelationId";
    private readonly RequestDelegate _next;

    public GlobalCorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items[CorrelationIdKey] = correlationId;
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);

        using (LogContext.PushProperty(CorrelationIdKey, correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}

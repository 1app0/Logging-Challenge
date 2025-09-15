using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace app.logging.Middleware;

public class LogContextProvider(ILogger<LogContextProvider> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = GetCorrelationId(context); 

        context.Response.Headers["x-correlation-id"] = correlationId;
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedTimeInMs = stopwatch.ElapsedMilliseconds;
                logger.LogInformation("Request {method}, Path {url} executed in {durationTimeMs} ms", context.Request.Method,
                    context.Request.Path, elapsedTimeInMs);
            }
        }
    }

    private string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue("x-correlation-id", out var correlationId);

        return correlationId.FirstOrDefault() ?? Guid.NewGuid().ToString();
    }
}

public static class LogContextProviderMiddlewareExtension
{
    public static IApplicationBuilder UseLogContextProviderMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<LogContextProvider>();
}
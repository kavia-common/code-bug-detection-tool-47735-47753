using System.Net;
using System.Text.Json;

namespace BugDetectorBackend.Middleware
{
    /// <summary>
    /// Centralized error handling middleware that logs exceptions and returns a standardized JSON error.
    /// </summary>
    public sealed class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
                // Request aborted by the client; do not log as an error.
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await WriteErrorAsync(context, "Request was cancelled by the client.", "REQ_CANCELLED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await WriteErrorAsync(context, "An unexpected error occurred.", "UNHANDLED_ERROR");
            }
        }

        private static async Task WriteErrorAsync(HttpContext context, string message, string code)
        {
            context.Response.ContentType = "application/json";
            var payload = new
            {
                error = new
                {
                    code,
                    message,
                    traceId = context.TraceIdentifier
                }
            };
            var json = JsonSerializer.Serialize(payload);
            await context.Response.WriteAsync(json);
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        // PUBLIC_INTERFACE
        /// <summary>
        /// Registers centralized error handling for the pipeline.
        /// </summary>
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}

namespace AMLSurvey.API.Middlewares
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;
        private readonly IHostEnvironment _env = env;

        // 🚀 Cached Base ProblemDetails for Production
        private static readonly ProblemDetails BaseProductionProblemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // 🚀 Structured Logging
            _logger.LogError(exception,
                "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                httpContext.TraceIdentifier,
                httpContext.Request.Path,
                httpContext.Request.Method);

            var problemDetails = _env.IsDevelopment()
                ? CreateDevelopmentProblemDetails(exception, httpContext)
                : CreateProductionProblemDetails(httpContext);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
            return true;
        }

        // ✅ Development: تفاصيل كاملة للمطورين
        private static ProblemDetails CreateDevelopmentProblemDetails(Exception exception, HttpContext context)
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = exception.Message,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Instance = context.Request.Path,
                Extensions =
                {
                    ["traceId"] = context.TraceIdentifier,
                    ["errorType"] = exception.GetType().Name,
                    ["stackTrace"] = exception.StackTrace,
                    ["innerException"] = exception.InnerException?.Message
                }
            };
        }

        // 🔒 Production: نسخة خفيفة + TraceId للتتبع
        private static ProblemDetails CreateProductionProblemDetails(HttpContext context)
        {
            var problem = new ProblemDetails
            {
                Status = BaseProductionProblemDetails.Status,
                Title = BaseProductionProblemDetails.Title,
                Type = BaseProductionProblemDetails.Type,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;
            return problem;
        }
    }
}

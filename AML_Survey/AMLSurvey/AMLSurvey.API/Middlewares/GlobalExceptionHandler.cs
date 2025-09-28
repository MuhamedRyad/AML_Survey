

namespace AMLSurvey.API.Middlewares
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;
        private readonly IHostEnvironment _env = env;
        //ValueTask => العمليات غالبًا تنتهي فورًا
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // تسجيل الخطأ في الـ Logs
            _logger.LogError(exception, "Something went wrong: {Message}", exception.Message);

            // إنشاء Response يعتمد على البيئة
            var problemDetails = _env.IsDevelopment()
                ? new ProblemDetails  //built in class
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = exception.Message,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    Extensions =
                    {
                        ["errorType"] = exception.GetType().Name,
                        ["stackTrace"] = exception.StackTrace
                    }
                }
                : new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error" // نسخة بسيطة للمستخدم النهائي
                };

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);

            return true;
        }
    }

}

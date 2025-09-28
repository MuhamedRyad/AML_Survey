

using AMLSurvey.API.Middlewares;
using AMLSurvey.Infrastructure.Extensions;

namespace AMLSurvey.API.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // API Layer Services
            services.AddApiServices(configuration);
            
            // Add Core Layer Services
            services.AddCoreServices();

            // Add Infrastructure Layer Services
            services.AddInfrastructureServices(configuration);

            return services;
        }

        private static IServiceCollection AddApiServices(this IServiceCollection services , IConfiguration configuration)
        {
            services.AddControllers();
            services.AddSwaggerDocumentation();
            services.AddOpenApi();

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            // CORS Configuration
            services.AddCustomCors(configuration);

            // API Versioning
            // services.AddApiVersioning();

            // Rate Limiting
            // services.AddRateLimiter();

            return services;
        }

        private static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // Business Services
            // services.AddScoped<ISurveyService, SurveyService>();
            // services.AddScoped<IUserService, UserService>();

            // AutoMapper
            // services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // FluentValidation
            // services.AddFluentValidationAutoValidation();
            // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // MediatR for CQRS
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }

        public static IApplicationBuilder ConfigureMiddleware(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            // ?? Exception Handler should be FIRST!
            app.UseExceptionHandler();

            if (env.IsDevelopment())
            {
                app.UseSwaggerDocumentation();
            }

           /* app.UseSerilogRequestLogging();*/

            app.UseHttpsRedirection();

            //app.UseCors();
            app.UseCustomCors();
            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseExceptionHandler(); built-in middleware
/*            app.UseMiddleware<ExceptionMiddleware>(); // custom middleware
*/
           /* app.UseRateLimiter();

            app.MapHealthChecks("health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

             app.UseHangfireDashboard
             RecurringJob.AddOrUpdate
              */

            return app;
        }
    }
}
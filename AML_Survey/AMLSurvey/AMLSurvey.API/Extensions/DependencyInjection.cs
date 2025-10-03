

using AMLSurvey.API.Middlewares;
using AMLSurvey.Core.Extensions;
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

            //connection to db
                   /* var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(connectionString));
                   */

            // API Versioning
            // services.AddApiVersioning();

            // Rate Limiting
            // services.AddRateLimiter();

            return services;
        }

  /*      private static IServiceCollection AddCoreServices(this IServiceCollection services)
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
        }*/

        public static IApplicationBuilder ConfigureMiddleware(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            // ✅ Correct middleware order
            
            // 1. Exception handling (MUST BE FIRST!)
            app.UseExceptionHandler();

            // 2. HTTPS redirection
            app.UseHttpsRedirection();

            // 3. CORS (before Authentication)
            app.UseCustomCors();

            // 4. Swagger (Development only)
            if (env.IsDevelopment())
            {
                app.UseSwaggerDocumentation();
            }

            // 5. Authentication & Authorization (MUST be in this order!)
            app.UseAuthentication();
            app.UseAuthorization();

            // TODO: Add more middleware
            // app.UseRateLimiter();
            // app.UseSerilogRequestLogging();

            
            /*app.UseExceptionHandler(); built-in middleware
                        app.UseMiddleware<ExceptionMiddleware>(); // custom middleware



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
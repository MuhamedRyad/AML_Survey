

using AMLSurvey.Core.Models;

namespace AMLSurvey.API.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsSettings = configuration.GetSection("CorsSettings");
            var policyName = corsSettings["PolicyName"] ?? "DefaultPolicy";
            var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            //  IF NULL IN APPSETTING  SET DEFAULT("GET", "POST")
            var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST" };
            //  IF NULL IN APPSETTING  SET DEFAULT ALLOW HEADER("Content-Type")
            var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "Content-Type" };
            //allow(Cookies,Authorization)
            var allowCredentials = corsSettings.GetValue<bool>("AllowCredentials", false);
            var maxAge = corsSettings.GetValue<int>("MaxAge", 86400);

            // Validation
            if (!allowedOrigins.Any())
            {
                throw new InvalidOperationException("No allowed origins defined in configuration.");
            }

            services.AddCors(options =>
            {
                //GET policyName FROM APPSETTING
                options.AddPolicy(policyName, builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .WithMethods(allowedMethods)
                           .WithHeaders(allowedHeaders)
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(maxAge));

                    if (allowCredentials)
                    {
                        builder.AllowCredentials();
                    }
                });
            });

            // Register config for dependency injection
            services.AddSingleton(new CorsPolicyConfig()
            {
                PolicyName = policyName,
                AllowedOrigins = allowedOrigins,
                AllowCredentials = allowCredentials
            });

            return services;
        }

        public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
        {
            var corsConfig = app.ApplicationServices.GetRequiredService<CorsPolicyConfig>();
            app.UseCors(corsConfig.PolicyName);
            return app;
        }
    }

}

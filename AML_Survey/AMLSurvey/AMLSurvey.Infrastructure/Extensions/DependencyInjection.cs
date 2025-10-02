using AMLSurvey.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Oracle.EntityFrameworkCore.Internal;

namespace AMLSurvey.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add required dependencies
            services.AddHttpContextAccessor();

            // Configure database
            services.AddDatabaseConfiguration(configuration);

            // Configure Identity
            services.AddAuthConfig(configuration);

            // Register repositories and services
            services.AddRepositoryPattern();
            services.AddCustomInfrastructureServices();

            return services;
        }



        private static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException(
                                       "Connection string 'DefaultConnection' not found.");

            // Pool size based on CPU cores
            var cpuCore = Environment.ProcessorCount;
            var poolSize = cpuCore * 2;
/*            var maxPoolSize = configuration.GetValue<int>("Database:MaxPoolSize", 64);
            var poolSize = Math.Min(cpuCore * 2, maxPoolSize);*/


            // services.AddDbContext<ApplicationContext>(options =>
            services.AddDbContextPool<ApplicationContext>(options =>
            {
                options.UseOracle(connectionString, oracleOptions =>
                {
                    oracleOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    oracleOptions.CommandTimeout(30);
                });

                // Performance optimizations
                options.EnableServiceProviderCaching(true);
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);

                // Warnings
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));

                // Query optimization
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

             
            }, poolSize); // Adjust pool size based on your needs  use it automatically
            return services;
        }

        public static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
        {
            // 1️⃣ Configure Identity
            //AddIdentityApiEndpoints  => endpoints by default will created
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>  
            {
                // Password settings
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false;

                // ✅ Lockout settings (Security Enhancement)
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
                options.Lockout.MaxFailedAccessAttempts = 3;  // Stricter than default
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationContext>() // DbContext
            .AddDefaultTokenProviders();

            // 2️⃣ Authorization Handlers & Policies
       /*     services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            // 3️⃣ JWT Options
            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            // 4️⃣ Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience
                };
            });

            // 5️⃣ JWT Provider (Custom)
            services.AddSingleton<IJwtProvider, JwtProvider>();*/

            return services;
        }


        private static IServiceCollection AddRepositoryPattern(this IServiceCollection services)
        {
            // Generic repository pattern
           /* services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();*/

            // Specific repositories can be added here
            // services.AddScoped<ISurveyRepository, SurveyRepository>();
            // services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }

        private static IServiceCollection AddCustomInfrastructureServices(this IServiceCollection services)
        {
            // Infrastructure services
           /* services.AddScoped<IEmailService, EmailService>();
              services.AddScoped<IFileService, FileService>();
              services.AddScoped<IAuditService, AuditService>();
           */

            // Background services
            // services.AddHostedService<BackgroundTaskService>();

            // Caching
            services.AddMemoryCache();
            // services.AddStackExchangeRedisCache(options => { ... });

            return services;
        }

        public static WebApplication MapIdentityEndpoints(this WebApplication app)
        {
            // Map Identity API endpoints


            /*var authGroup = app.MapGroup("api")  //:5001/api/register
                .WithTags()
                .WithOpenApi();*/

           // app.MapIdentityApi<ApplicationUser>(); //:5001/register


            // Custom Identity endpoints can be mapped here
            // app.MapPost("api/auth/refresh-token", RefreshTokenEndpoint);

            /* var identityGroup = app.MapGroup("api/account")

                 // ??? ???????? (Authentication) ??? ???? ??? endpoints ???? ??? ????????
                 .RequireAuthorization()

                 // ????? ?????? Identity ?? Swagger ??? ????? ?? ????? ???? "Identity"
                 .WithTags("Identity")

                 // ????? ??? ???????? ???????? ?? ????? Swagger/OpenAPI
                 .WithOpenApi()

                 // ????? ????? CORS ????? ???? "AllowAngular" ?????? ??? Frontend (??? Angular) ???????
                 .RequireCors("AllowAngular")

                 // ????? ????? Rate Limiting ???? "fixed" ???? ????????? ?????? ?? ???????
                 .RequireRateLimiting("fixed")

                 // ????? ???? ???? (LoggingFilter) ?????? ?? ??? ????? ??? ????? ?? Endpoint
                 .AddEndpointFilter<LoggingFilter>();

             // ??? Endpoints ?????? ?? Identity (??? register, login, logout...) ????????? ???????
             identityGroup.MapIdentityApi<AppUser>();*/

            return app;
        }
    }
}
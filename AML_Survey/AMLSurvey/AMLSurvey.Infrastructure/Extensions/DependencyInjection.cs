
namespace AMLSurvey.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Configuration
          /*  services.AddDbContext<ApplicationDbContext>(options =>
                options.UseOracle(configuration.GetConnectionString("DefaultConnection")));*/

            // Repository Pattern
            // services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            // services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Infrastructure Services
            // services.AddScoped<IEmailService, EmailService>();
            // services.AddScoped<IFileService, FileService>();

            return services;
        }
    }
}
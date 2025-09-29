using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace AMLSurvey.Core.Extensions
{
    public static class DependencyInjection
    {


        /// تسجيل كل الـ Core services (Mapster, FluentValidation, Application Services)
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddMapsterConfiguration();
            services.AddFluentValidationConfiguration();

            // لو عندك Application Services
            // services.AddScoped<IStudentService, StudentService>();

            return services;
        }



        /// إعداد Mapster للـ object mapping
        private static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;

            // مسح كل الـ IRegister mappings في الـ Core assembly
            config.Scan(typeof(DependencyInjection).Assembly);

            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }

        

        /// إعداد FluentValidation للـ automatic validation
        private static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
        {
            services
                .AddFluentValidationAutoValidation() // Model State Validation تلقائياً
                .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
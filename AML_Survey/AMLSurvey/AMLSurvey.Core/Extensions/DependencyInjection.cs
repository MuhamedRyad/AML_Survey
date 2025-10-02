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
        private static readonly Assembly _coreAssembly = typeof(DependencyInjection).Assembly;

        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddMapsterConfiguration();
            services.AddFluentValidationConfiguration();

            return services;
        }

        //mapster
        private static readonly Lazy<TypeAdapterConfig> _mapsterConfig = new(() =>
        {
            var config = new TypeAdapterConfig();
            config.Scan(_coreAssembly);

            config.Compile(); //Precompile mappings for max performance
            return config;
        });

        private static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
        {

            services.AddSingleton(_mapsterConfig.Value);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }

        

        // automatic validation
        private static IServiceCollection AddFluentValidationConfiguration(this IServiceCollection services)
        {
            services
                .AddFluentValidationAutoValidation() // Model State Validation تلقائياً
                .AddValidatorsFromAssembly(_coreAssembly);  //Cached Assembly

            return services;
        }
    }
}
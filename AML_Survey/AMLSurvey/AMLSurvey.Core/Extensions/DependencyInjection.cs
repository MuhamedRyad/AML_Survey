using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMLSurvey.Core.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // 1️⃣ إعداد Mapster
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly()); // يسجل كل IRegister في Core
            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            // 2️⃣ تسجيل باقي الـ services
            //services.AddScoped<IStudentService, StudentService>();

            return services;
        }
    }
}

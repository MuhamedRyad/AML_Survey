

namespace AMLSurvey.API.Extensions
{
    public static class SwaggerServiceExtensions
    {

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            
            services.AddEndpointsApiExplorer();

          
            services.AddSwaggerGen(c =>
            {
                
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AML Survey Website API",           
                    Version = "v1",                           
                    Description = "My API for managing an AML Survey website.", 
                    Contact = new OpenApiContact              
                    {
                        Name = "Mo Reyad",
                        Email = "support@ecommerce.com"
                    }
                });

                
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath); // إدراج تعليقات XML في مستند Swagger
                }
            });

            // يعيد services لتكملة سلسلة الإعدادات في Program.cs
            return services;
        }

        // امتداد لتفعيل Swagger وواجهة المستخدم الخاصة به (Swagger UI)
        // يتم استدعاؤها مثل: app.UseSwaggerDocumentation();
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            // تفعيل خدمة Swagger middleware لإنشاء وثيقة JSON
            app.UseSwagger();

            // تفعيل واجهة المستخدم الخاصة بـ Swagger (Swagger UI)
            app.UseSwaggerUI(c =>
            {
                // تحديد نقطة النهاية التي ستُستخدم لقراءة وثيقة Swagger
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceWebsite API V1");

                // تحديد مسار الواجهة (Swagger UI) - يمكن فتحها على /swagger
                c.RoutePrefix = "swagger"; // إذا أردت فتحها على الجذر استخدم string.Empty
            });

            // يعيد app لتكملة سلسلة الـ middleware في Program.cs
            return app;
        }
    }
}

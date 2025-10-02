

using AMLSurvey.API.Extensions;
using AMLSurvey.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure all services through single entry point
    builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
app.ConfigureMiddleware();
app.MapIdentityEndpoints(); // Map Identity Endpoints after middleware configuration
app.MapControllers();

app.Run();

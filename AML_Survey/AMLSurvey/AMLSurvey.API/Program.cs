

using AMLSurvey.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure all services through single entry point
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
app.ConfigureMiddleware();

app.MapControllers();

app.Run();

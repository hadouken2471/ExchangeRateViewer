using ExchangeRateViewer.Users.Api.Endpoints;
using ExchangeRateViewer.Users.Api.Extensions;
using ExchangeRateViewer.Users.Api.Middleware;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Сервис пользователей",
        Version = "v1",
        Description = "Регистрация, аутентификация и управление сессиями пользователей (JWT)"
    });
});
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services.AddUsersApplication();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });
    app.MapScalarApiReference(options =>
    {
        options.EndpointPathPrefix = "/docs/{documentName}";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("api").MapUserEndpoints();

await app.RunAsync();

using ExchangeRateViewer.Finance.Api.Endpoints;
using ExchangeRateViewer.Finance.Api.Extensions;
using ExchangeRateViewer.Finance.Api.Middleware;
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
        Title = "Сервис финансов",
        Version = "v1",
        Description = "Курсы валют ЦБ РФ и управление избранными валютами пользователя"
    });
});
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddFinanceInfrastructure(builder.Configuration);
builder.Services.AddFinanceApplication();

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

app.MapGroup("api").MapFinanceEndpoints();

await app.RunAsync();

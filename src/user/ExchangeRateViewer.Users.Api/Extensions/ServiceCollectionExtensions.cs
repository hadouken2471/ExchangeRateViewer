using System.Text;
using ExchangeRateViewer.Shared.Cqrs;
using ExchangeRateViewer.Users.Application.Commands;
using ExchangeRateViewer.Users.Application.Models;
using ExchangeRateViewer.Users.Application.Options;
using ExchangeRateViewer.Users.Application.Services;
using ExchangeRateViewer.Users.Domain.Entities;
using ExchangeRateViewer.Users.Infrastructure.Data;
using ExchangeRateViewer.Users.Infrastructure.Repositories;
using ExchangeRateViewer.Users.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ExchangeRateViewer.Users.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(jwtSection);

        var jwtOptions = jwtSection.Get<JwtOptions>()
                         ?? throw new InvalidOperationException("Конфигурация JWT не найдена");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenStore, TokenStore>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<ICommandHandler<RegisterCommand, AuthDto>, RegisterCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, AuthDto>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutCommand, bool>, LogoutCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, AuthDto>, RefreshTokenCommandHandler>();

        return services;
    }
}

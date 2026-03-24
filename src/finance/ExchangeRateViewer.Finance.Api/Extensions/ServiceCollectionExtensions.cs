using System.Text;
using ExchangeRateViewer.Finance.Api.Options;
using ExchangeRateViewer.Finance.Application.Commands;
using ExchangeRateViewer.Finance.Application.Models;
using ExchangeRateViewer.Finance.Domain.Entities;
using ExchangeRateViewer.Finance.Infrastructure.Data;
using ExchangeRateViewer.Finance.Infrastructure.Queries;
using ExchangeRateViewer.Finance.Infrastructure.Repositories;
using ExchangeRateViewer.Shared.Cqrs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ExchangeRateViewer.Finance.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
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

    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FinanceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IFavoriteCurrencyRepository, FavoriteCurrencyRepository>();

        return services;
    }

    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ICommandHandler<AddFavoriteCommand, bool>, AddFavoriteCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveFavoriteCommand, bool>, RemoveFavoriteCommandHandler>();
        services.AddScoped<IQueryHandler<GetRatesQuery, List<CurrencyDto>>, GetRatesQueryHandler>();
        services.AddScoped<IQueryHandler<GetFavoritesQuery, List<CurrencyDto>>, GetFavoritesQueryHandler>();

        return services;
    }
}

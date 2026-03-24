using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExchangeRateViewer.Finance.Application.Commands;
using ExchangeRateViewer.Finance.Application.Models;
using ExchangeRateViewer.Finance.Infrastructure.Queries;
using ExchangeRateViewer.Shared.Cqrs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRateViewer.Finance.Api.Endpoints;

public static class FinanceEndpoints
{
    private static async Task<Ok<List<CurrencyDto>>> GetRatesList(
        [FromServices] IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var rates = await queryDispatcher.Dispatch<GetRatesQuery, List<CurrencyDto>>(
            new GetRatesQuery(), cancellationToken);
        return TypedResults.Ok(rates);
    }

    private static async Task<Results<Ok<List<CurrencyDto>>, UnauthorizedHttpResult>> GetFavoritesList(
        HttpContext context,
        [FromServices] IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        var favorites = await queryDispatcher.Dispatch<GetFavoritesQuery, List<CurrencyDto>>(
            new GetFavoritesQuery(userId), cancellationToken);
        return TypedResults.Ok(favorites);
    }

    private static async Task<Results<Ok, UnauthorizedHttpResult>> AddFavorite(
        HttpContext context,
        [FromRoute] string currencyId,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        await commandDispatcher.Dispatch<AddFavoriteCommand, bool>(
            new AddFavoriteCommand(userId, currencyId), cancellationToken);
        return TypedResults.Ok();
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> RemoveFavorite(
        HttpContext context,
        [FromRoute] string currencyId,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(context, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        await commandDispatcher.Dispatch<RemoveFavoriteCommand, bool>(
            new RemoveFavoriteCommand(userId, currencyId), cancellationToken);
        return TypedResults.NoContent();
    }

    public static void MapFinanceEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGroup("/finance")
            .BuildFinanceEndpoints()
            .WithTags("Финансы");
    }

    private static RouteGroupBuilder BuildFinanceEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGet("/rates", GetRatesList)
            .WithSummary("Все курсы валют")
            .WithDescription("Возвращает актуальные курсы всех валют ЦБ РФ, отсортированные по названию.")
            .Produces<List<CurrencyDto>>(200);

        builder.MapGet("/favorites", GetFavoritesList)
            .RequireAuthorization()
            .WithSummary("Избранные валюты")
            .WithDescription("Возвращает список избранных валют текущего пользователя.")
            .Produces<List<CurrencyDto>>(200)
            .Produces(401);

        builder.MapPost("/favorites/{currencyId}", AddFavorite)
            .RequireAuthorization()
            .WithSummary("Добавить в избранное")
            .WithDescription("Добавляет валюту в избранное по буквенному коду (например USD, EUR).")
            .Produces(200)
            .Produces(401)
            .ProducesProblem(404)
            .ProducesProblem(409);

        builder.MapDelete("/favorites/{currencyId}", RemoveFavorite)
            .RequireAuthorization()
            .WithSummary("Удалить из избранного")
            .WithDescription("Удаляет валюту из избранного текущего пользователя.")
            .Produces(204)
            .Produces(401)
            .ProducesProblem(404);

        return builder;
    }

    private static bool TryGetUserId(HttpContext context, out Guid userId)
    {
        userId = Guid.Empty;
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return claim is not null && Guid.TryParse(claim, out userId);
    }
}

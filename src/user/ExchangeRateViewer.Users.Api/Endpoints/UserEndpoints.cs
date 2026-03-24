using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExchangeRateViewer.Shared.Cqrs;
using ExchangeRateViewer.Users.Api.Models;
using ExchangeRateViewer.Users.Application.Commands;
using ExchangeRateViewer.Users.Application.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRateViewer.Users.Api.Endpoints;

public static class UserEndpoints
{
    private static async Task<Ok<AuthDto>> Register(
        [FromBody] RegisterRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Dispatch<RegisterCommand, AuthDto>(
            new RegisterCommand(request.Name, request.Password), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<AuthDto>> Login(
        [FromBody] LoginRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Dispatch<LoginCommand, AuthDto>(
            new LoginCommand(request.Name, request.Password), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, UnauthorizedHttpResult>> Logout(
        HttpContext context,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (jti is null || userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
        {
            return TypedResults.Unauthorized();
        }

        await commandDispatcher.Dispatch<LogoutCommand, bool>(
            new LogoutCommand(jti, userId), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<AuthDto>> Refresh(
        [FromBody] RefreshRequest request,
        [FromServices] ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Dispatch<RefreshTokenCommand, AuthDto>(
            new RefreshTokenCommand(request.AccessToken, request.RefreshToken), cancellationToken);
        return TypedResults.Ok(result);
    }

    public static void MapUserEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapGroup("/users")
            .BuildUserEndpoints()
            .WithTags("Пользователи");
    }

    private static RouteGroupBuilder BuildUserEndpoints(this RouteGroupBuilder builder)
    {
        builder.MapPost("/register", Register)
            .WithSummary("Регистрация")
            .WithDescription("Создаёт нового пользователя и возвращает пару JWT-токенов (access + refresh).")
            .Produces<AuthDto>(200)
            .ProducesProblem(409);

        builder.MapPost("/login", Login)
            .WithSummary("Вход")
            .WithDescription("Аутентификация по имени и паролю. Возвращает пару JWT-токенов.")
            .Produces<AuthDto>(200)
            .ProducesProblem(400);

        builder.MapPost("/logout", Logout)
            .RequireAuthorization()
            .WithSummary("Выход")
            .WithDescription("Отзывает текущий access token и удаляет все refresh token пользователя.")
            .Produces(204)
            .Produces(401);

        builder.MapPost("/token/refresh", Refresh)
            .WithSummary("Обновление токенов")
            .WithDescription("Обменивает истёкший access token и валидный refresh token на новую пару токенов.")
            .Produces<AuthDto>(200)
            .ProducesProblem(400);

        return builder;
    }
}

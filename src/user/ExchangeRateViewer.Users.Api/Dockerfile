FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS restore
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["src/user/ExchangeRateViewer.Users.Api/ExchangeRateViewer.Users.Api.csproj", "src/user/ExchangeRateViewer.Users.Api/"]
COPY ["src/user/ExchangeRateViewer.Users.Application/ExchangeRateViewer.Users.Application.csproj", "src/user/ExchangeRateViewer.Users.Application/"]
COPY ["src/user/ExchangeRateViewer.Users.Domain/ExchangeRateViewer.Users.Domain.csproj", "src/user/ExchangeRateViewer.Users.Domain/"]
COPY ["src/user/ExchangeRateViewer.Users.Infrastructure/ExchangeRateViewer.Users.Infrastructure.csproj", "src/user/ExchangeRateViewer.Users.Infrastructure/"]
COPY ["src/shared/ExchangeRateViewer.Shared.Cqrs/ExchangeRateViewer.Shared.Cqrs.csproj", "src/shared/ExchangeRateViewer.Shared.Cqrs/"]
COPY ["src/shared/ExchangeRateViewer.Shared.Kernel/ExchangeRateViewer.Shared.Kernel.csproj", "src/shared/ExchangeRateViewer.Shared.Kernel/"]
RUN dotnet restore "src/user/ExchangeRateViewer.Users.Api/ExchangeRateViewer.Users.Api.csproj"

FROM restore AS publish
COPY . .
RUN dotnet publish "src/user/ExchangeRateViewer.Users.Api/ExchangeRateViewer.Users.Api.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRateViewer.Users.Api.dll"]

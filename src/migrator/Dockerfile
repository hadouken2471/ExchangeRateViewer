FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS restore
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["src/migrator/ExchangeRateViewer.Migrator.csproj", "src/migrator/"]
COPY ["src/user/ExchangeRateViewer.Users.Infrastructure/ExchangeRateViewer.Users.Infrastructure.csproj", "src/user/ExchangeRateViewer.Users.Infrastructure/"]
COPY ["src/user/ExchangeRateViewer.Users.Application/ExchangeRateViewer.Users.Application.csproj", "src/user/ExchangeRateViewer.Users.Application/"]
COPY ["src/user/ExchangeRateViewer.Users.Domain/ExchangeRateViewer.Users.Domain.csproj", "src/user/ExchangeRateViewer.Users.Domain/"]
COPY ["src/finance/ExchangeRateViewer.Finance.Infrastructure/ExchangeRateViewer.Finance.Infrastructure.csproj", "src/finance/ExchangeRateViewer.Finance.Infrastructure/"]
COPY ["src/finance/ExchangeRateViewer.Finance.Application/ExchangeRateViewer.Finance.Application.csproj", "src/finance/ExchangeRateViewer.Finance.Application/"]
COPY ["src/finance/ExchangeRateViewer.Finance.Domain/ExchangeRateViewer.Finance.Domain.csproj", "src/finance/ExchangeRateViewer.Finance.Domain/"]
COPY ["src/shared/ExchangeRateViewer.Shared.Kernel/ExchangeRateViewer.Shared.Kernel.csproj", "src/shared/ExchangeRateViewer.Shared.Kernel/"]
COPY ["src/shared/ExchangeRateViewer.Shared.Cqrs/ExchangeRateViewer.Shared.Cqrs.csproj", "src/shared/ExchangeRateViewer.Shared.Cqrs/"]
RUN dotnet restore "src/migrator/ExchangeRateViewer.Migrator.csproj"

FROM restore AS publish
COPY . .
RUN dotnet publish "src/migrator/ExchangeRateViewer.Migrator.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRateViewer.Migrator.dll"]

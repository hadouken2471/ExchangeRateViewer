FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS restore
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["src/finance/ExchangeRateViewer.Finance.Api/ExchangeRateViewer.Finance.Api.csproj", "src/finance/ExchangeRateViewer.Finance.Api/"]
COPY ["src/finance/ExchangeRateViewer.Finance.Application/ExchangeRateViewer.Finance.Application.csproj", "src/finance/ExchangeRateViewer.Finance.Application/"]
COPY ["src/finance/ExchangeRateViewer.Finance.Domain/ExchangeRateViewer.Finance.Domain.csproj", "src/finance/ExchangeRateViewer.Finance.Domain/"]
COPY ["src/finance/ExchangeRateViewer.Finance.Infrastructure/ExchangeRateViewer.Finance.Infrastructure.csproj", "src/finance/ExchangeRateViewer.Finance.Infrastructure/"]
COPY ["src/shared/ExchangeRateViewer.Shared.Cqrs/ExchangeRateViewer.Shared.Cqrs.csproj", "src/shared/ExchangeRateViewer.Shared.Cqrs/"]
COPY ["src/shared/ExchangeRateViewer.Shared.Kernel/ExchangeRateViewer.Shared.Kernel.csproj", "src/shared/ExchangeRateViewer.Shared.Kernel/"]
RUN dotnet restore "src/finance/ExchangeRateViewer.Finance.Api/ExchangeRateViewer.Finance.Api.csproj"

FROM restore AS publish
COPY . .
RUN dotnet publish "src/finance/ExchangeRateViewer.Finance.Api/ExchangeRateViewer.Finance.Api.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRateViewer.Finance.Api.dll"]

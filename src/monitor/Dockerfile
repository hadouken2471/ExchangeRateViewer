FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS restore
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["src/monitor/ExchangeRateMonitor.csproj", "src/monitor/"]
RUN dotnet restore "src/monitor/ExchangeRateMonitor.csproj"

FROM restore AS publish
COPY . .
RUN dotnet publish "src/monitor/ExchangeRateMonitor.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRateMonitor.dll"]

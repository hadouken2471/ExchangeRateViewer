FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS restore
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["src/gateway/ExchangeRateViewer.ApiGateway.csproj", "src/gateway/"]
RUN dotnet restore "src/gateway/ExchangeRateViewer.ApiGateway.csproj"

FROM restore AS publish
COPY . .
RUN dotnet publish "src/gateway/ExchangeRateViewer.ApiGateway.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRateViewer.ApiGateway.dll"]

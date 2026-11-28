FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY . .
RUN dotnet restore SpendAudit.API/SpendAudit.API.csproj
RUN dotnet publish SpendAudit.API/SpendAudit.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    Storage__RootPath=/app/uploads

COPY --from=build /app/publish .
RUN mkdir -p /app/uploads && chown -R app:app /app

EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "SpendAudit.API.dll"]
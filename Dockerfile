# FinTracker — multi-stage, кэш NuGet, проверка publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) Restore (слой кэшируется, пока не меняются .csproj)
COPY src/FinTracker.Domain/FinTracker.Domain.csproj src/FinTracker.Domain/
COPY src/FinTracker.Application/FinTracker.Application.csproj src/FinTracker.Application/
COPY src/FinTracker.Infrastructure/FinTracker.Infrastructure.csproj src/FinTracker.Infrastructure/
COPY src/FinTracker.Web/FinTracker.Web.csproj src/FinTracker.Web/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/FinTracker.Web/FinTracker.Web.csproj

# 2) Исходники и publish
COPY src/FinTracker.Domain/ src/FinTracker.Domain/
COPY src/FinTracker.Application/ src/FinTracker.Application/
COPY src/FinTracker.Infrastructure/ src/FinTracker.Infrastructure/
COPY src/FinTracker.Web/ src/FinTracker.Web/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/FinTracker.Web/FinTracker.Web.csproj \
    && dotnet publish src/FinTracker.Web/FinTracker.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    && mkdir -p /app/publish/wwwroot/_framework \
    && cp -r "$(ls -d /root/.nuget/packages/microsoft.aspnetcore.app.internal.assets/*/_framework | head -1)/." /app/publish/wwwroot/_framework/ \
    && test -f /app/publish/FinTracker.Web.staticwebassets.endpoints.json \
    && test -f /app/publish/wwwroot/_framework/blazor.web.js

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# curl + пользователь (кэшируется отдельно от приложения)
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && (getent group app >/dev/null || groupadd --gid 1000 app) \
    && (getent passwd app >/dev/null || useradd --uid 1000 --gid app --system --no-create-home app)

COPY --from=build --chown=app:app /app/publish .

USER app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=45s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "FinTracker.Web.dll"]

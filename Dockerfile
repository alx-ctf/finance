FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/FinTracker.Domain/ src/FinTracker.Domain/
COPY src/FinTracker.Application/ src/FinTracker.Application/
COPY src/FinTracker.Infrastructure/ src/FinTracker.Infrastructure/
COPY src/FinTracker.Web/ src/FinTracker.Web/

RUN dotnet restore src/FinTracker.Web/FinTracker.Web.csproj
RUN dotnet publish src/FinTracker.Web/FinTracker.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinTracker.Web.dll"]

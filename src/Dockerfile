FROM mcr.microsoft.com/dotnet/core/aspnet:2.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build
WORKDIR /src

# Keep the project list and command dotnet restore identical in all Dockfiles to maximize image cache utilization
COPY AspNetCoreApiStarter.sln .
COPY Web.Api/Web.Api.csproj /src/Web.Api/
COPY Web.Api.Core/Web.Api.Core.csproj /src/Web.Api.Core/
COPY Web.Api.Infrastructure/Web.Api.Infrastructure.csproj /src/Web.Api.Infrastructure/
RUN dotnet restore AspNetCoreApiStarter.sln

COPY . .
WORKDIR /src/Web.Api
RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Web.Api.dll"]

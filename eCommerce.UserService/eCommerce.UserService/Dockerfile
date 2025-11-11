# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["eCommerce.UserService/eCommerce.UserService/eCommerce.API.csproj", "eCommerce.UserService/eCommerce.UserService/"]
COPY ["eCommerce.Shared/eCommerce.Shared/eCommerce.Shared.csproj", "eCommerce.Shared/eCommerce.Shared/"]
COPY ["eCommerce.UserService/eCommerce.Core/eCommerce.Core.csproj", "eCommerce.UserService/eCommerce.Core/"]
RUN dotnet restore "./eCommerce.UserService/eCommerce.UserService/eCommerce.API.csproj"
COPY . .
WORKDIR "/src/eCommerce.UserService/eCommerce.UserService"
RUN dotnet build "./eCommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./eCommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
Env POSTGRES_HOST=postgres
Env POSTGRES_PORT=5432
Env POSTGRES_DATABASE=eCommerce.User
Env POSTGRES_USERNAME=postgres
Env POSTGRES_PASSWORD=admin
ENTRYPOINT ["dotnet", "eCommerce.API.dll"]
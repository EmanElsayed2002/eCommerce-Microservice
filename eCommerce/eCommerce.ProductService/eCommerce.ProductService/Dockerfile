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
COPY ["eCommerce.ProductService/eCommerce.ProductService/eCommerce.ProductService.csproj", "eCommerce.ProductService/eCommerce.ProductService/"]
COPY ["eCommerce.ProductService/eCommerce.Core/eCommerce.Core.csproj", "eCommerce.ProductService/eCommerce.Core/"]
COPY ["eCommerce.Shared/eCommerce.Shared/eCommerce.Shared.csproj", "eCommerce.Shared/eCommerce.Shared/"]
COPY ["eCommerce.ProductService/eCommerce.Domain/eCommerce.Domain.csproj", "eCommerce.ProductService/eCommerce.Domain/"]
RUN dotnet restore "./eCommerce.ProductService/eCommerce.ProductService/eCommerce.ProductService.csproj"
COPY . .
WORKDIR "/src/eCommerce.ProductService/eCommerce.ProductService"
RUN dotnet build "./eCommerce.ProductService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./eCommerce.ProductService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV MYSQL_HOST=localhost
ENV MYSQL_PASSWORD=admin
ENV MYSQL_DATABASE=ecommerceproductsdatabase
ENV MYSQL_USER=root
ENV MYSQL_PORT=3306
ENTRYPOINT ["dotnet", "eCommerce.ProductService.dll"]
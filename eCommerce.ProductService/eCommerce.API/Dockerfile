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

# Copy project files first for better layer caching
COPY ["eCommerce.Shared/eCommerce.Shared/eCommerce.Shared.csproj", "eCommerce.Shared/eCommerce.Shared/"]
COPY ["eCommerce.ProductService/eCommerce.DataAccessLayer/eCommerce.DataAccessLayer.csproj", "eCommerce.ProductService/eCommerce.DataAccessLayer/"]
COPY ["eCommerce.ProductService/eCommerce.BusinessLogicLayer/eCommerce.BusinessLogicLayer.csproj", "eCommerce.ProductService/eCommerce.BusinessLogicLayer/"]
COPY ["eCommerce.ProductService/eCommerce.API/eCommerce.API.csproj", "eCommerce.ProductService/eCommerce.API/"]

# Restore dependencies
RUN dotnet restore "./eCommerce.ProductService/eCommerce.API/eCommerce.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the project
WORKDIR "/src/eCommerce.ProductService/eCommerce.API"
RUN dotnet build "./eCommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./eCommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV MYSQL_HOST=localhost
ENV MYSQL_PASSWORD=admin
ENV MYSQL_DATABASE=ecommerceproductsdatabase
ENV MYSQL_USER=root
ENV MYSQL_PORT=3306
# RabbitMQ Configuration
# For docker-compose: use the service name (e.g., "rabbitmq")
# For standalone container: use container name or IP
# For RabbitMQ on host machine: use "host.docker.internal" (Windows/Mac) or host IP (Linux)
ENTRYPOINT ["dotnet", "eCommerce.API.dll"]
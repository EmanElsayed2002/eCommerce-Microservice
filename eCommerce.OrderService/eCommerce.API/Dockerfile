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
COPY ["eCommerce.OrderService/eCommerce.DataAccessLayer/eCommerce.DataAccessLayer.csproj", "eCommerce.OrderService/eCommerce.DataAccessLayer/"]
COPY ["eCommerce.OrderService/eCommerce.BusinessLogicLayer/eCommerce.BusinessLogicLayer.csproj", "eCommerce.OrderService/eCommerce.BusinessLogicLayer/"]
COPY ["eCommerce.OrderService/eCommerce.API/eCommerce.API.csproj", "eCommerce.OrderService/eCommerce.API/"]

# Restore dependencies
RUN dotnet restore "./eCommerce.OrderService/eCommerce.API/eCommerce.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the project
WORKDIR "/src/eCommerce.OrderService/eCommerce.API"
RUN dotnet build "./eCommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./eCommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV MONGODB_HOST=mongodb
ENV MONGODB_PORT=27017
ENV MONGODB_DATABASE=OrdersDatabase
ENV RabbitMQ_HostName=rabbitmq
ENV RabbitMQ_UserName=guest
ENV RabbitMQ_Password=guest
ENV RabbitMQ_Port=5672
ENV RabbitMQ_Exchange=order.exchange
ENV RabbitMQ_RoutingKey=order.routingkey
ENV RabbitMQ_Products_Exchange=product.exchange
ENTRYPOINT ["dotnet", "eCommerce.API.dll"]
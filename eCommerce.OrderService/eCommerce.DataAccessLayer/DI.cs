using eCommerce.DataAccessLayer.Repository;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace eCommerce.DataAccessLayer
{
    public static class DI
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services , IConfiguration configuration)
        {
            // Get base connection string template (e.g., "mongodb://$MONGO_HOST:$MONGO_PORT")
            string connectionStringTemplate = configuration.GetConnectionString("MongoDB")
                ?? "mongodb://$MONGO_HOST:$MONGO_PORT";

            // Read env vars with sane local fallbacks
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST")
                ?? configuration["MongoDB:Host"]
                ?? "localhost";
            var port = Environment.GetEnvironmentVariable("MONGODB_PORT")
                ?? configuration["MongoDB:Port"]
                ?? "27017";
            var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE")
                ?? configuration["MongoDB:Database"]
                ?? "OrdersDatabase";

            // Build final connection string safely
            string connectionString = connectionStringTemplate
                .Replace("$MONGO_HOST", host)
                .Replace("$MONGO_PORT", port);

            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

            services.AddScoped<IMongoDatabase>(provider =>
            {
                IMongoClient client = provider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            services.AddScoped<IOrderRepository, OrderRepository>();
            return services;
        }
    }
}

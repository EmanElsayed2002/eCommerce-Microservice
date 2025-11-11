using eCommerce.DataAccessLayer.MyDbContext;
using eCommerce.DataAccessLayer.Repository;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.DataAccessLayer
{
    public static class DI
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection service , IConfiguration config)
        {
            var connectionStringTemplate = config.GetConnectionString("MySqlConnection") 
                ?? throw new InvalidOperationException("MySqlConnection string is not configured in appsettings.json");
            
            // Read from environment variables first, then fall back to configuration
            var host = Environment.GetEnvironmentVariable("MYSQL_HOST") 
                ?? config["MySQL:Host"] 
                ?? throw new InvalidOperationException("MySQL host is required. Set MYSQL_HOST environment variable or MySQL:Host in appsettings.json");
            
            var port = Environment.GetEnvironmentVariable("MYSQL_PORT") 
                ?? config["MySQL:Port"] 
                ?? "3306";
            
            var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") 
                ?? config["MySQL:Database"] 
                ?? throw new InvalidOperationException("MySQL database name is required. Set MYSQL_DATABASE environment variable or MySQL:Database in appsettings.json");
            
            var user = Environment.GetEnvironmentVariable("MYSQL_USER") 
                ?? config["MySQL:User"] 
                ?? throw new InvalidOperationException("MySQL user is required. Set MYSQL_USER environment variable or MySQL:User in appsettings.json");
            
            var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") 
                ?? config["MySQL:Password"] 
                ?? throw new InvalidOperationException("MySQL password is required. Set MYSQL_PASSWORD environment variable or MySQL:Password in appsettings.json");
            
            // Replace placeholders in connection string template
            var connectionString = connectionStringTemplate
                .Replace("$MYSQL_HOST", host)
                .Replace("$MYSQL_PORT", port)
                .Replace("$MYSQL_DATABASE", database)
                .Replace("$MYSQL_USER", user)
                .Replace("$MYSQL_PASSWORD", password);
            
            service.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySQL(connectionString);
            });
            service.AddScoped<IProductRepo, ProductRepo>();
            service.AddScoped<AppDbContext>();
            return service;
        }

    }
}

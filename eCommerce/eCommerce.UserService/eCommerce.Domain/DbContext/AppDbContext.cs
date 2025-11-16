using System.Data;
using System.Data.Common;

using eCommerce.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace eCommerce.Infrastructure.DbContext;

public class AppDbContext
{
    public readonly IDbConnection dbConnection;
    public  AppDbContext(IConfiguration config)
    {
        var connectionStringTemplate = config.GetConnectionString("PostgresConnection");
        
        if (string.IsNullOrWhiteSpace(connectionStringTemplate))
        {
            throw new InvalidOperationException("PostgresConnection string is not configured in appsettings.json");
        }

        // Get environment variables with fallback to configuration
        // Support both POSTGRES_USERNAME and POSTGRES_USER for compatibility
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? config["Postgres:Host"] ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? config["Postgres:Port"] ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? config["Postgres:Database"] ?? throw new InvalidOperationException("PostgreSQL database name is required. Set POSTGRES_DATABASE environment variable or Postgres:Database in appsettings.json");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USERNAME") ?? Environment.GetEnvironmentVariable("POSTGRES_USER") ?? config["Postgres:Username"] ?? throw new InvalidOperationException("PostgreSQL username is required. Set POSTGRES_USERNAME environment variable or Postgres:Username in appsettings.json");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? config["Postgres:Password"] ?? throw new InvalidOperationException("PostgreSQL password is required. Set POSTGRES_PASSWORD environment variable or Postgres:Password in appsettings.json");

        var connectionstring = connectionStringTemplate
            .Replace("$POSTGRES_HOST", host)
            .Replace("$POSTGRES_PORT", port)
            .Replace("$POSTGRES_DATABASE", database)
            .Replace("$POSTGRES_USERNAME", username)
            .Replace("$POSTGRES_PASSWORD", password);

        dbConnection = new Npgsql.NpgsqlConnection(connectionstring);
    }

    public IDbConnection _dbContext => dbConnection;
    

}

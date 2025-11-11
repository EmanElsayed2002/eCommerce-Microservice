using eCommerce.BusinessLogicLayer.HttpClientt;
using eCommerce.BusinessLogicLayer.Policies;
using eCommerce.BusinessLogicLayer.RabbitMQ;
using eCommerce.BusinessLogicLayer.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.BusinessLogicLayer
{
    public static class DI
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration? configuration = null)
        {
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IRabbitMqPublisher>(provider => 
            {
                var config = configuration ?? provider.GetRequiredService<IConfiguration>();
                return new RabbitMqPublisher(config);
            });
            
            // Register Polly Policies
            services.AddScoped<IPollyPolicies, PollyPolicies>();
            services.AddScoped<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();
            services.AddScoped<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
            
            services.AddScoped<ProductsMicroserviceClient>();
            services.AddScoped<UsersMicroserviceClient>();
            
            // Register RabbitMQ Consumers (Singleton - they run continuously)
            services.AddSingleton<IRabbitMQProductDeletionConsumer, RabbitMQProductDeletionConsumer>();
            services.AddSingleton<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>();
            
            // Register Hosted Services to start consumers when application starts
            services.AddHostedService<RabbitMQProductDeletionHostedService>();
            services.AddHostedService<RabbitMQProductNameUpdateHostedService>();
            
            return services;
        }
    }
}


using eCommerce.BusinessLogicLayer.RabbitMQ;
using eCommerce.BusinessLogicLayer.Services;
using eCommerce.Core.RabbitMQ;

using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.BusinessLogicLayer
{
    public static class DI
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
           services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddScoped<IRabbitMQOrderCreatedConsumer, RabbitMQOrderCreatedConsumer>();
            services.AddScoped<IRabbitMQOrderDeletedConsumer, RabbitMQOrderDeletedConsumer>();
            services.AddScoped<IRabbitMQOrderUpdatedConsumer, RabbitMQOrderUpdatedConsumer>();
            services.AddHostedService<RabbitMQOrderCreatedHostedService>();
            services.AddHostedService<RabbitMQOrderDeletedHostedService>();
            services.AddHostedService<RabbitMQOrderUpdatedHostedService>();
           
            return services;
        }
    }
}

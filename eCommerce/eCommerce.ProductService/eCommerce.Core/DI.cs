using eCommerce.BusinessLogicLayer.RabbitMQ;
using eCommerce.BusinessLogicLayer.Services;

using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.BusinessLogicLayer
{
    public static class DI
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
           services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
           
            return services;
        }
    }
}

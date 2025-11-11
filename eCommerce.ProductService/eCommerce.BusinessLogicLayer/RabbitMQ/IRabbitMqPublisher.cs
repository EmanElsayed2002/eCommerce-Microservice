using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMqPublisher
    {
        Task Publish<T>(Dictionary<string, object> headers, T message);
    }
}

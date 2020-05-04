using Common.Bus;
using Common.Attributes;

namespace Producer.Models
{
    [Queue(queueName: "Person", exchangeName: "ecommerce_bus", routingKey: "Person")]
    public class User : IntegrationEvent
    {
        public string Name { get; set; }
    }
}

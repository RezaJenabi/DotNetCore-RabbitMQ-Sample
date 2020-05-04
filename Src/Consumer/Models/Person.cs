using Common.Bus;
using Common.Attributes;

namespace Consumer.Models
{
    [Queue(queueName: "Person", exchangeName: "ecommerce_bus", routingKey: "Person")]
    public class Person : IntegrationEvent
    {
        public string Name { get; set; }
    }
}

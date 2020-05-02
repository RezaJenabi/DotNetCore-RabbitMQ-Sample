using Common.Attributes;
using Common.Bus;

namespace Producer.Models
{
    [Queue(queueName: "Person", exchangeName: "Exchange_Sample", routingKey: "Exchange_Sample_Person")]
    public class Person : IntegrationEvent
    {
        public string Name { get; set; }
    }
}

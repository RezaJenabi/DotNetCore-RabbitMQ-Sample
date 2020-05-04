using Common.Attributes;
using Common.Bus.RabbitMQ;
using Consumer.Models;
using System.Diagnostics;

namespace Consumer.RebbitMQ.IRabbitMQ
{
    [Queue(queueName: "Person", exchangeName: "ecommerce_bus", routingKey: "Person")]
    public class UserInsert: IConsumer<Person>
    {
        public void Consume(Person message)
        {
            Debug.WriteLine("Eeeeeeeeeeeeeeeeeeeeeeeee");
        }
    }
}

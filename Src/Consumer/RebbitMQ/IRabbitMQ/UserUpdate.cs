using System.Collections.Generic;
using Consumer.Models;
using Common.Attributes;
using Common.Bus.RabbitMQ;

namespace Consumer.RebbitMQ.IRabbitMQ
{
    [Queue(queueName: "QueueUserUpdate", exchangeName: "ExchangeUserUpdate", routingKey: "UserUpdate_Id")]
    public class UserUpdate : IConsumer<List<Person>>
    {
        public void Consume(List<Person> message)
        {

        }
    }
}

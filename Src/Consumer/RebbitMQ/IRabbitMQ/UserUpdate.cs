using System.Collections.Generic;
using Consumer.Models;
using Consumer.RebbitMQ.Attributes;

namespace Consumer.RebbitMQ.IRabbitMQ
{
    [Queue(queueName: "QueueUserUpdate", exchangeName: "ExchangeUserUpdate", routingKey: "UserUpdate_Id")]
    public class UserUpdate : IConsumer<List<User>>
    {
        public void Consume(List<User> message)
        {
            
        }
    }
}

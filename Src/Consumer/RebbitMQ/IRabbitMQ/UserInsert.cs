using System.Collections.Generic;
using Consumer.Models;
using Consumer.RebbitMQ.Attributes;

namespace Consumer.RebbitMQ.IRabbitMQ
{
    [Queue(queueName: "QueueMessageService", exchangeName: "ExchangeMessageService", routingKey: "MessageService_Id")]
    public class UserInsert: IConsumer<List<User>>
    {
        public void Consume(List<User> message)
        {
            
        }
    }
}

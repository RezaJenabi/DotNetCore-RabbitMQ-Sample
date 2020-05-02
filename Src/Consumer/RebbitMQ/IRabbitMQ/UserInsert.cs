using System.Collections.Generic;
using Consumer.Models;
using Common.Attributes;
using Common.Bus.RabbitMQ;

namespace Consumer.RebbitMQ.IRabbitMQ
{
    [Queue(queueName: "Person", exchangeName: "ExchangeMessageService", routingKey: "MessageService_Id")]
    public class UserInsert: IConsumer<User>
    {
        public void Consume(User message)
        {
            
        }
    }
}

using Consumer.Models;

namespace Consumer.RebbitMQ.IRabbitMQ
{
    public class UserInsert: IConsumer<User>
    {
        public void Consume(User message)
        {
            
        }
    }
}

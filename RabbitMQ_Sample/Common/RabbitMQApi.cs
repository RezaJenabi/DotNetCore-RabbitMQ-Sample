using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ_Sample.ViewModel;

namespace RabbitMQ_Sample.Common
{
    public class RabbitMQApi : DefaultBasicConsumer, IRabbitMQApi
    {
        public ConnectionFactory Factory { get; set; }= new ConnectionFactory
        {
            UserName = "guest",
            Password = "guest",
            Port = 5672,
            HostName = "localhost",
            VirtualHost = "/"
        };
        //you can change ConnectionFactory
        //new ConnectionFactory
        //{
        //    Uri = new Uri("amqp://guest:guest@localhost:5672/vhost"); //Pattern => "amqp://user:pass@hostName:port/vhost"
        //};


        public PublishResult PublishDirect(PublishRequest publish)
        {
            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("messageexchange", ExchangeType.Direct);
                channel.QueueDeclare(publish.RoutingKey, true, false, false, null);
                channel.QueueBind(publish.RoutingKey, "messageexchange", publish.RoutingKey, null);
                channel.BasicPublish("messageexchange", publish.RoutingKey, null, Encoding.UTF8.GetBytes(publish.Body));
            }
           
            return new PublishResult { Status = true };
        }

        public SubscribeResult SubscribeDirect(SubscribeRequest subscribeRequest)
        {
            var subscribeResult = new SubscribeResult(); ;
            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.QueueDeclare(queue: subscribeRequest.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                var result = channel.BasicGet(queue: subscribeRequest.Queue, autoAck: false);
                if (result != null)
                {
                    subscribeResult.Body = Encoding.UTF8.GetString(result.Body);
                }
            }
            return subscribeResult;
        }

        public PublishResult PublishTopic(PublishRequest publish)
        {
            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var properties = channel.CreateBasicProperties();

                properties.Persistent = false;

                var messagebuffer = Encoding.Default.GetBytes("Message from Topic Exchange");

                channel.BasicPublish("topic.exchange", "Message.Bombay.Email", properties, messagebuffer);
            }

            return new PublishResult { Status = true };
        }

        public SubscribeResult SubscribeTopic(SubscribeRequest subscribeRequest)
        {
            var subscribeResult = new SubscribeResult(); ;
            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.BasicQos(0, 1, false);
                var result = channel.BasicGet(queue: "topic.bombay.queue", autoAck: false);
                if (result != null)
                {
                    subscribeResult.Body = Encoding.UTF8.GetString(result.Body);
                }

            }
            return subscribeResult;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)

        {

            Console.WriteLine($"Consuming Topic Message");

            Console.WriteLine(string.Concat("Message received from the exchange ", exchange));

            Console.WriteLine(string.Concat("Consumer tag: ", consumerTag));

            Console.WriteLine(string.Concat("Delivery tag: ", deliveryTag));

            Console.WriteLine(string.Concat("Routing tag: ", routingKey));

            Console.WriteLine(string.Concat("Message: ", Encoding.UTF8.GetString(body)));


        }

        //check State queue
        public QueueDeclareOk PassiveDeclaration(SubscribeRequest subscribeRequest)
        {
            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                return channel.QueueDeclarePassive(subscribeRequest.Queue);
            }
        }

    }
}

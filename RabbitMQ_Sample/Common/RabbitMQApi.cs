using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ_Sample.ViewModel;

namespace RabbitMQ_Sample.Common
{
    public class RabbitMQApi : IRabbitMQApi
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


        public PublishResult Publish(PublishRequest publish)
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

        public SubscribeResult Subscribe(SubscribeRequest subscribeRequest)
        {
            List<string> g=new List<string>();
            var subscribeResult = new SubscribeResult(); ;

            using (var connection = Factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.QueueDeclare(queue: subscribeRequest.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                var result = channel.BasicGet(queue: subscribeRequest.Queue, autoAck: true);
                if (result != null)
                {
                    subscribeResult.Body = Encoding.UTF8.GetString(result.Body);
                }
            }

            
            return subscribeResult;
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

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Collections.Generic;
using System.Text;
using Consumer.Models;
using Newtonsoft.Json;

namespace Consumer.RebbitMQ.IRabbitMQ
{
    public class AutoSubscriber : IAutoSubscriber
    {
        private readonly IConnectionFactory _connectionFactory;
        IConnection _connection;
        bool _disposed;
        private readonly IAutoSubscriber _persistentConnection;
        private IModel _consumerChannel;
        private string _queueName;

        public AutoSubscriber(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            if (!IsConnected)
            {
                TryConnect();
            }
        }

        public void Subscribe(Assembly getExecutingAssembly)
        {
            if (!IsConnected)
            {
                TryConnect();
            }
            Consumer();

        }

        public void SubscribeAsync(Assembly getExecutingAssembly)
        {
            if (!IsConnected)
            {
                TryConnect();
            }

           // //CHannel 02
           // _eventBusRabbitMQ = new EventBusRabbitMQ(this, "emailSendMsgQ");
            AsyncConsumer();

          // //CHannel 01
           // _eventBusRabbitMQ = new EventBusRabbitMQ(this, "userInsertMsgQ");
          //  _eventBusRabbitMQ.CreateConsumerChannel();

           // //CHannel 02
           // _eventBusRabbitMQ = new EventBusRabbitMQ(this, "emailSendMsgQ");
           // _eventBusRabbitMQ.CreateConsumerChannel();
        }
        public IModel Consumer()
        {
            if (!IsConnected)
            {
                TryConnect();
            }

            var channel = CreateModel();
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            //consumer.Received += async (model, e) =>
            //{
            //    var eventName = e.RoutingKey;
            //    var message = Encoding.UTF8.GetString(e.Body);
            //    channel.BasicAck(e.DeliveryTag, multiple: false);
            //};

            //Create event when something receive
            consumer.Received += ReceivedEvent;

            channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = Consumer();
            };
            return channel;
        }

        public IModel AsyncConsumer()
        {
            if (!IsConnected)
            {
                TryConnect();
            }

            var channel = CreateModel();
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, e) =>
            {
                var eventName = e.RoutingKey;
                var message = Encoding.UTF8.GetString(e.Body);
                channel.BasicAck(e.DeliveryTag, multiple: false);
            };


            channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = AsyncConsumer();
            };
            return channel;
        }
        private void ReceivedEvent(object sender, BasicDeliverEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Body);

            if (e.RoutingKey == "userInsertMsgQ")
            {
                //var message = Encoding.UTF8.GetString(e.Body);
                List<User> userList = JsonConvert.DeserializeObject<List<User>>(message);
               // UserSaveFeedback saveFeedback = _userService.InsertUsers(userList);

               // PublishUserSaveFeedback("userInsertMsgQ_feedback", saveFeedback, e.BasicProperties.Headers);
            }

            if (e.RoutingKey == "emailSendMsgQ")
            {
                //Implementation here
            }
        }

        public void PublishUserSaveFeedback(string _queueName, UserSaveFeedback publishModel, IDictionary<string, object> headers)
        {

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {

                channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                var message = JsonConvert.SerializeObject(publishModel);
                var body = Encoding.UTF8.GetBytes(message);

                IBasicProperties properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.DeliveryMode = 2;
                properties.Headers = headers;
                // properties.Expiration = "36000000";
                //properties.ContentType = "text/plain";

                channel.ConfirmSelect();
                channel.BasicPublish(exchange: "", routingKey: _queueName, mandatory: true, basicProperties: properties, body: body);
                channel.WaitForConfirmsOrDie();

                channel.BasicAcks += (sender, eventArgs) =>
                {
                    Console.WriteLine("Sent RabbitMQ");
                    //implement ack handle
                };
                channel.ConfirmSelect();
            }
        }



        public void Disconnect()
        {
            if (_disposed)
            {
                return;
            }
            Dispose();
        }
        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool TryConnect()
        {

            try
            {
                Console.WriteLine("RabbitMQ Client is trying to connect");
                _connection = _connectionFactory.CreateConnection();
            }
            catch (BrokerUnreachableException e)
            {
                Thread.Sleep(5000);
                Console.WriteLine("RabbitMQ Client is trying to reconnect");
                _connection = _connectionFactory.CreateConnection();
            }

            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                Console.WriteLine($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                return true;
            }
            else
            {
                //  implement send warning email here
                //-----------------------
                Console.WriteLine("FATAL ERROR: RabbitMQ connections could not be created and opened");
                return false;
            }

        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            Console.WriteLine("A RabbitMQ connection is shutdown. Trying to re-connect...");
            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            Console.WriteLine("A RabbitMQ connection throw exception. Trying to re-connect...");
            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;
            Console.WriteLine("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            TryConnect();
        }


    }
}

using Common.Attributes;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common.Bus.RabbitMQ
{
    public class BusRabbitMQ : IBus, IDisposable
    {
        private readonly IRabbitMQConnection _persistentConnection;
        private MethodInfo _consumeMethod;
        private IModel _consumerChannel;
        private string _queueName;
        const string BROKER_NAME = "ecommerce_bus";
        private object _consumer;

        public BusRabbitMQ(IRabbitMQConnection persistentConnection)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        }


        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            Init(@event);

            using (_consumerChannel = CreateChannel())
            {
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);
                var properties = _consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                _consumerChannel.BasicPublish(
                    exchange: BROKER_NAME,
                    routingKey: _queueName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            }
        }

        public void Subscribe(Assembly getExecutingAssembly)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var items = getExecutingAssembly.GetExportedTypes().Where(x => x.IsClass).ToList();
            items.ForEach(x =>
            {
                if (x.GetInterfaces().All(y => y != typeof(IConsumer))) return;
                var consumer = x.GetConstructor(Type.EmptyTypes);
                foreach (Attribute attribute in x.GetCustomAttributes(true))
                {
                    if (!(attribute is QueueAttribute queue)) continue;
                    _queueName = queue.QueueName ?? x.Name;

                    if (consumer != null) _consumer = consumer.Invoke(new object[] { });

                    _consumeMethod = x.GetMethod("Consume");

                    Consumer();
                }
            });
        }

        public void SubscribeAsync(Assembly getExecutingAssembly)
        {

        }

        private IModel Consumer()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = CreateChannel();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ReceivedEvent;
            channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = Consumer();
            };
            return channel;
        }

        private IModel AsyncConsumer()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = CreateChannel();
            channel.QueueDeclare(_queueName, true, false, false, null);

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

            var items = Assembly.GetExecutingAssembly()?.GetExportedTypes().Where(x => x.IsClass).ToList();
            items.ForEach(x =>
            {
                if (x.GetInterfaces().All(y => y != typeof(IConsumer))) return;
                var consumer = x.GetConstructor(Type.EmptyTypes);

                foreach (Attribute attribute in x.GetCustomAttributes(true))
                {
                    if (consumer != null) _consumer = consumer.Invoke(new object[] { });

                    _consumeMethod = x.GetMethod("Consume");

                    if (_queueName != e.RoutingKey) continue;

                    if (_consumeMethod != null)
                        _consumeMethod.Invoke(_consumer, new object[]
                        {
                            JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(e.Body),
                                _consumeMethod.GetParameters().FirstOrDefault()?.ParameterType)
                        });
                }
            });
        }

        //private void PublishUserSaveFeedback(string _queueName, UserSaveFeedback publishModel, IDictionary<string, object> headers)
        //{

        //    if (!_persistentConnection.IsConnected)
        //    {
        //        _persistentConnection.TryConnect();
        //    }

        //    using (var channel = _persistentConnection.CreateModel())
        //    {

        //        channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        //        var message = JsonConvert.SerializeObject(publishModel);
        //        var body = Encoding.UTF8.GetBytes(message);

        //        IBasicProperties properties = channel.CreateBasicProperties();
        //        properties.Persistent = true;
        //        properties.DeliveryMode = 2;
        //        properties.Headers = headers;
        //        // properties.Expiration = "36000000";
        //        //properties.ContentType = "text/plain";

        //        channel.ConfirmSelect();
        //        channel.BasicPublish(exchange: "", routingKey: _queueName, mandatory: true, basicProperties: properties, body: body);
        //        channel.WaitForConfirmsOrDie();

        //        channel.BasicAcks += (sender, eventArgs) =>
        //        {
        //            Console.WriteLine("Sent RabbitMQ");
        //            //implement ack handle
        //        };
        //        channel.ConfirmSelect();
        //    }
        //}

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }
        }

        private IModel CreateChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: BROKER_NAME, type: ExchangeType.Direct);
            channel.QueueDeclare(_queueName, true, false, false, null);
            channel.QueueBind(_queueName, BROKER_NAME, _queueName, null);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateChannel();
            };

            return channel;
        }

        private void Init(IntegrationEvent @event)
        {
            foreach (Attribute attribute in @event.GetType().GetCustomAttributes(true))
            {
                if (!(attribute is QueueAttribute queue)) continue;
                _queueName = queue.QueueName ?? @event.GetType().Name;
            }
        }
       
    }
}

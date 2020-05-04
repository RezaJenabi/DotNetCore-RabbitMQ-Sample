using Common.Attributes;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private Assembly _getExecutingAssembly;

        public BusRabbitMQ(IRabbitMQConnection persistentConnection)
        {
            Debug.WriteLine("BusRabbitMQ Constractor");

            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        }


        public void Publish(IntegrationEvent @event)
        {
            Debug.WriteLine("BusRabbitMQ Publish");

            using (_consumerChannel = CreateChannel(@event.GetType()))
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
            Debug.WriteLine("BusRabbitMQ Subscribe");

            _getExecutingAssembly = getExecutingAssembly;

            var items = getExecutingAssembly.GetExportedTypes().Where(x => x.IsClass).ToList();
            items.ForEach(x =>
            {
                if (x.GetInterfaces().Any(y => y == typeof(IConsumer)))
                {
                    var consumer = x.GetConstructor(Type.EmptyTypes);
                    if (consumer != null) _consumer = consumer.Invoke(new object[] { });
                    _consumeMethod = x.GetMethod("Consume");
                    Consumer(x);
                }

            });
        }

        public void SubscribeAsync(Assembly getExecutingAssembly)
        {
            _getExecutingAssembly = getExecutingAssembly;
        }

        private IModel Consumer(Type @event)
        {
            Debug.WriteLine("BusRabbitMQ Consumer");

            using (_consumerChannel = CreateChannel(@event))
            {
                var consumer = new EventingBasicConsumer(_consumerChannel);
                consumer.Received += ReceivedEvent;
                _consumerChannel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
                _consumerChannel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = Consumer(@event);
            };

                return _consumerChannel;
            }
        }

        private IModel AsyncConsumer(Type @event)
        {
            using (_consumerChannel = CreateChannel(@event))
            {
                var consumer = new EventingBasicConsumer(_consumerChannel);

                consumer.Received += async (model, e) =>
                {
                    var eventName = e.RoutingKey;
                    var message = Encoding.UTF8.GetString(e.Body);
                    _consumerChannel.BasicAck(e.DeliveryTag, multiple: false);
                };

                _consumerChannel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
                _consumerChannel.CallbackException += (sender, ea) =>
                {
                    _consumerChannel.Dispose();
                    _consumerChannel = AsyncConsumer(@event);
                };
                return _consumerChannel;
            }

        }

        private void ReceivedEvent(object sender, BasicDeliverEventArgs e)
        {
            Debug.WriteLine("BusRabbitMQ ReceivedEvent");

            var items = _getExecutingAssembly?.GetExportedTypes().Where(x => x.IsClass).ToList();
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
                    {
                        Debug.WriteLine("BusRabbitMQ _consumeMethod");
                        _consumeMethod.Invoke(_consumer, new object[]
                      {
                            JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Body),
                                _consumeMethod.GetParameters().FirstOrDefault()?.ParameterType)
                      });
                    }

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

        private IModel CreateChannel(Type @event)
        {
            Init(@event);
            _persistentConnection.TryConnect();

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: BROKER_NAME, type: ExchangeType.Direct);
            channel.QueueDeclare(_queueName, true, false, false, null);
            channel.QueueBind(_queueName, BROKER_NAME, _queueName, null);

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateChannel(@event);
            };

            return channel;
        }

        private void Init(Type @event)
        {
            foreach (Attribute attribute in @event.GetCustomAttributes(true))
            {
                if (!(attribute is QueueAttribute queue)) continue;
                _queueName = queue.QueueName ?? @event.Name;
            }
        }

    }
}

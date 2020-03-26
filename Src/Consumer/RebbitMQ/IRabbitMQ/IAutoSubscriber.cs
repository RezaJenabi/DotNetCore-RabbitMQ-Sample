using RabbitMQ.Client;
using System;
using System.Reflection;

namespace Consumer.RebbitMQ
{
    public interface IAutoSubscriber : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();

        void Disconnect();
        void Subscribe(string subscriptionId,Assembly getExecutingAssembly);
        void SubscribeAsync(Assembly getExecutingAssembly);
    }
}

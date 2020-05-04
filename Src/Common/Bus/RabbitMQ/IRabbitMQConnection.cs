using RabbitMQ.Client;
using System;

namespace Common.Bus.RabbitMQ
{
    public interface IRabbitMQConnection : IDisposable
    {
        bool TryConnect();
        IModel CreateModel();
    }
}

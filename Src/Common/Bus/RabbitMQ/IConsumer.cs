using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Bus.RabbitMQ
{
    public interface IConsumer<in T> : IConsumer where T : class
    {
        void Consume(T message);
    }

    public interface IConsumer
    {
    }
}

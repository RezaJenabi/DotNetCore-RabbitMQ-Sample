using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consumer.RebbitMQ.IRabbitMQ
{
    interface IConsumer<in T>: IConsumer where T : class
    {
        void Consume(T message);
    }

    interface IConsumer
    {
    }
}

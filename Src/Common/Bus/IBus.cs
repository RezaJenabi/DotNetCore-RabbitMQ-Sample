using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Common.Bus
{
    public interface IBus
    {
        void Publish(IntegrationEvent @event);
        void Subscribe(Assembly getExecutingAssembly);
        void SubscribeAsync(Assembly getExecutingAssembly);
    }
}

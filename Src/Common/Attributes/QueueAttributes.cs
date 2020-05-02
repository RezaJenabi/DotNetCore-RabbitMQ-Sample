using System;

namespace Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class QueueAttribute : Attribute
    {
        public QueueAttribute(string queueName = null, string exchangeName = null, string routingKey = null)
        {
            QueueName = queueName;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
        }

        public string QueueName { get; }
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }
    }
}

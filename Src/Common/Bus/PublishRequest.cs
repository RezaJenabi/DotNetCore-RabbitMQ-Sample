using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Bus
{
    public class PublishRequest
    {
        public string Body { get; set; }
        public string RoutingKey { get; set; }
    }

    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public IntegrationEvent(Guid id, DateTime createDate)
        {
            Id = id;
            CreationDate = createDate;
        }
        public Guid Id { get; private set; }
        public DateTime CreationDate { get; private set; }
    }
}

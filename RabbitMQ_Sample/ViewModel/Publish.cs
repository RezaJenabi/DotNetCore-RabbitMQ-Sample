using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ_Sample.ViewModel
{
    public class PublishResult
    {
        public bool Status { get; set; }
    }

    public class PublishRequest
    {
        public string Body { get; set; }
        public string RoutingKey { get; set; }
    }
}

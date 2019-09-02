using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ_Sample.Common;
using RabbitMQ_Sample.ViewModel;

namespace RabbitMQ_Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IRabbitMQApi _rabbitMqApi;

        public MessagesController(IRabbitMQApi rabbitMqApi)
        {
            _rabbitMqApi = rabbitMqApi;
        }
        [HttpGet]
        [Route("Send")]
        public JsonResult Send()
        {
            var publishResult = _rabbitMqApi.Publish(new PublishRequest{ Body = "text message" , RoutingKey = "Queue2"});
            return null;
        }
        [HttpGet]
        [Route("Receive")]
        public string Receive()
        {
                var subscribeResult = _rabbitMqApi.Subscribe(new SubscribeRequest{Queue = "Queue3" });
                return subscribeResult?.Body;
        }

        [HttpGet]
        [Route("PassiveDeclaration")]
        public void PassiveDeclaration()
        {
            _rabbitMqApi.PassiveDeclaration(new SubscribeRequest { Queue = "Queue1" });
        }
    }
}

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
    public class TopicController : ControllerBase
    {
        private readonly IRabbitMQApi _rabbitMqApi;
        private string Key { get; } = "Pdf_Log_Queue";

        public TopicController(IRabbitMQApi rabbitMqApi)
        {
            _rabbitMqApi = rabbitMqApi;
        }
        [HttpGet]
        [Route("Send")]
        public JsonResult Send()
        {
            var publishResult = _rabbitMqApi.PublishTopic(new PublishRequest { Body = "Pdf_Events", RoutingKey = Key });
            return new JsonResult(publishResult);
        }
        [HttpGet]
        [Route("Receive")]
        public JsonResult Receive()
        {
            var subscribeResult = _rabbitMqApi.SubscribeTopic(new SubscribeRequest { Queue = Key });
            return new JsonResult(subscribeResult);
        }

    }
}

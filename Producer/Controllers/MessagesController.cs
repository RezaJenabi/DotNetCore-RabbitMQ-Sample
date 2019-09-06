using Common.RabbitMQ;
using Common.ViewModel;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Consumer.Models;
using Consumer.Service;

namespace Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IRabbitMQApi _rabbitMqApi;
        private string Key { get; } = "Pdf_Log_Queue";

        public MessagesController(IRabbitMQApi rabbitMqApi)
        {
            _rabbitMqApi = rabbitMqApi;
        }
        [HttpGet]
        [Route("Send")]
        public JsonResult Send()
        {
            var publishResult = _rabbitMqApi.PublishDirect(new PublishRequest { Body = "Pdf_Events", RoutingKey = Key });
            return new JsonResult(publishResult);
        }

        [HttpGet]
        [Route("Receive")]
        public JsonResult Receive()
        {
            var subscribeResult = _rabbitMqApi.SubscribeDirect(new SubscribeRequest { Queue = Key });
            return new JsonResult(subscribeResult);
        }

        [HttpGet]
        [Route("text")]
        public void text()
        {


            {
                string senderUniqueId = "userInsertMsgQ";

                var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
                var connection = factory.CreateConnection();

                //-------------------------  Sending Data --------------------------------------------------------------------------------------
                #region Sending Data
                using (var channel = connection.CreateModel())
                {

                    channel.QueueDeclare(queue: "userInsertMsgQ", durable: false, exclusive: false, autoDelete: false, arguments: null);


                    // create serialize object to send
                    UserService _userService = new UserService();
                    List<User> objeUserList = new List<User> { new User { EmailAddress = "d", FirstName = "d", LastName = "dd" } };
                    string message = JsonConvert.SerializeObject(objeUserList);

                    var body = Encoding.UTF8.GetBytes(message);
                    //var body = "[{FirstName='a',LastName='d'}]";

                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;
                    properties.Headers = new Dictionary<string, object>
                    {
                        { "senderUniqueId", senderUniqueId }//optional unique sender details in receiver side              
                    };
                    // properties.Expiration = "36000000";
                    //properties.ContentType = "text/plain";

                    channel.ConfirmSelect();
                    channel.BasicPublish(exchange: "",
                                         routingKey: "userInsertMsgQ",
                                         false,
                                         basicProperties: properties,
                                         body: body);

                    channel.WaitForConfirmsOrDie();

                    channel.BasicAcks += (sender, eventArgs) =>
                    {
                        //implement ack handle
                    };
                    channel.ConfirmSelect();

                }
                #endregion


                //-------------------------  Receiving feedback ---------------------------------------------------------------------------------
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "userInsertMsgQ_feedback",
                                       durable: false,
                                       exclusive: false,
                                       autoDelete: false,
                                       arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        IDictionary<string, object> headers = ea.BasicProperties.Headers; // get headers from Received msg

                        foreach (KeyValuePair<string, object> header in headers)
                        {
                            if (senderUniqueId == Encoding.UTF8.GetString((byte[])header.Value))// Get feedback message only for me
                            {
                                var body = ea.Body;
                                var message = Encoding.UTF8.GetString(body);
                                UserSaveFeedback feedback = JsonConvert.DeserializeObject<UserSaveFeedback>(message);
                                // Console.WriteLine("[x] Feedback received ... ");
                                //Console.WriteLine("[x] Success count {0} and failed count {1}", feedback.successCount, feedback.failedCount);
                            }
                        }
                    };

                    channel.BasicConsume(queue: "userInsertMsgQ_feedback",
                                         autoAck: true,
                                         consumer: consumer);



                }
            }
        }
    }
}
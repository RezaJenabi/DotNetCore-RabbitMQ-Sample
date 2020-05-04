using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Consumer.Models;
using Common.Bus;
using Producer.Models;

namespace Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IBus _bus;
        
        public MessagesController(IBus bus)
        {
            _bus = bus;
        }
        [HttpGet]
        [Route("Send")]
        public JsonResult Send()
        {
            _bus.Publish(new User { Name = "Reza" });
            return new JsonResult(null);
        }

        [HttpGet]
        [Route("text")]
        public void text()
        {
            {
                const string senderUniqueId = "userInsertMsgQ";
                var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
                var connection = factory.CreateConnection();

                //-------------------------  Sending Data --------------------------------------------------------------------------------------
                #region Sending Data

                //var objUserList = new List<User> { new User { EmailAddress = "d", FirstName = "d", LastName = "dd" } };
                //using (var channel = connection.CreateModel())
                //{
                //    channel.QueueDeclare(queue: "userInsertMsgQ", durable: false, exclusive: false, autoDelete: false, arguments: null);
                //    // create serialize object to send
                //    var message = JsonConvert.SerializeObject(objUserList);

                //    var body = Encoding.UTF8.GetBytes(message);
                //    //var body = "[{FirstName='a',LastName='d'}]";

                //    var properties = channel.CreateBasicProperties();
                //    properties.Persistent = true;
                //    properties.DeliveryMode = 2;
                //    properties.Headers = new Dictionary<string, object>
                //    {
                //        { "senderUniqueId", senderUniqueId }//optional unique sender details in receiver side              
                //    };
                //    // properties.Expiration = "36000000";
                //    //properties.ContentType = "text/plain";

                //    channel.ConfirmSelect();
                //    channel.BasicPublish("",
                //                          "userInsertMsgQ",
                //                         false,
                //                         basicProperties: properties,
                //                         body: body);

                //    channel.WaitForConfirmsOrDie();

                //    channel.BasicAcks += (sender, eventArgs) =>
                //    {
                //        //implement ack handle
                //    };
                //    channel.ConfirmSelect();

                //}
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
                        var headers = ea.BasicProperties.Headers; // get headers from Received msg

                        foreach (KeyValuePair<string, object> header in headers)
                        {
                            if (senderUniqueId != Encoding.UTF8.GetString((byte[])header.Value)) continue;
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);
                            var feedback = JsonConvert.DeserializeObject<UserSaveFeedback>(message);
                            // Console.WriteLine("[x] Feedback received ... ");
                            //Console.WriteLine("[x] Success count {0} and failed count {1}", feedback.successCount, feedback.failedCount);
                        }
                    };

                    channel.BasicConsume(queue: "userInsertMsgQ_feedback", autoAck: true, consumer: consumer);
                }
            }
        }

        [HttpGet]
        [Route("Send2")]
        public void Send2()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ConfirmSelect();
            channel.ExchangeDeclare("ExchangeMessageService", ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
            channel.QueueDeclare("QueueMessageService", true, false, false, null);
            channel.QueueBind("QueueMessageService", "ExchangeMessageService", "MessageService_Id");
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.DeliveryMode = 2;

           // var objUserList = new List<User> { new User { EmailAddress = "d", FirstName = "d", LastName = "dd" } };

           // var message = JsonConvert.SerializeObject(objUserList);

           // var body = Encoding.UTF8.GetBytes(message);
          //  channel.BasicPublish("ExchangeMessageService", "MessageService_Id", properties, body);


          //  channel.ExchangeDeclare("ExchangeUserUpdate", ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
          //  channel.QueueDeclare("QueueUserUpdate", true, false, false, null);
          //  channel.QueueBind("QueueUserUpdate", "ExchangeUserUpdate", "UserUpdate_Id");

         //   channel.BasicPublish("ExchangeUserUpdate", "UserUpdate_Id", properties, body);

            // byte[] body = ...;
            // var properties = channel.CreateBasicProperties();
            // properties.Persistent = true;
            // properties.DeliveryMode = 2;
            // channel.BasicPublish("ExchangeMessageService", "MessageService_Id", properties, Encoding.UTF8.GetBytes("Hi"));
            // uses a 5 second timeout
            //channel.WaitForConfirmsOrDie(new TimeSpan(500));



            //Topics
            //channel.ExchangeDeclare("ExchangeLogs", ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
            //channel.QueueDeclare("Logs", true, false, false, null);
            //channel.QueueDeclare("Log_Inf", true, false, false, null);
            //channel.QueueDeclare("Log_Cri", true, false, false, null);

            //channel.QueueBind("Logs", "ExchangeLogs", "Log.#");
            //channel.QueueBind("Log_Inf", "ExchangeLogs", "Log.Inf");
            //channel.QueueBind("Log_Cri", "ExchangeLogs", "Log.Cri");

            //channel.BasicPublish("ExchangeLogs", "Log.Inf", null, Encoding.UTF8.GetBytes("Hi"));
            //channel.BasicPublish("ExchangeLogs", "Log.Cri", null, Encoding.UTF8.GetBytes("Hi"));


        }
    }

    
}
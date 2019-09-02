using RabbitMQ.Client;
using RabbitMQ_Sample.ViewModel;

namespace RabbitMQ_Sample.Common
{
  public  interface IRabbitMQApi
  {
      PublishResult Publish(PublishRequest publish);
      SubscribeResult Subscribe(SubscribeRequest subscribeRequest);
      QueueDeclareOk PassiveDeclaration(SubscribeRequest subscribeRequest);
  }
}

using RabbitMQ.Client;
using RabbitMQ_Sample.ViewModel;

namespace RabbitMQ_Sample.Common
{
    public interface IRabbitMQApi
    {
        PublishResult PublishDirect(PublishRequest publish);
        SubscribeResult SubscribeDirect(SubscribeRequest subscribeRequest);

        PublishResult PublishTopic(PublishRequest publish);
        SubscribeResult SubscribeTopic(SubscribeRequest subscribeRequest);

        QueueDeclareOk PassiveDeclaration(SubscribeRequest subscribeRequest);
    }
}

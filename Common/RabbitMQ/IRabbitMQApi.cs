using Common.ViewModel;
using RabbitMQ.Client;

namespace Common.RabbitMQ
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

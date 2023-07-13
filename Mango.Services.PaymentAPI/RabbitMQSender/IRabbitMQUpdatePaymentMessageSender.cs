using Mango.MessageBus;

namespace Mango.Services.PaymentAPI.RabbitMQSender
{
    public interface IRabbitMQUpdatePaymentMessageSender
    {
        void SendMessage(BaseMessage baseMessage, string queueName);
    }
}

using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Mango.MessageBus
{
    public class AzureServiceMessageBus : IMessageBus
    {
        private string connectionString = "Endpoint=sb://mangorestaurantweb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mxwGqADz37UWxvruxKXkq8Y39wJUA+FTV+ASbNSgx2Q=";
        private readonly ServiceBusClient _serviceBusClient;        

        public AzureServiceMessageBus()
        {            
            _serviceBusClient = new ServiceBusClient(connectionString);            
        }

        public async Task PublishMessage(BaseMessage message, string topicName)
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender(topicName);
                        
            var jsonMessage = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage);

            await sender.SendMessageAsync(serviceBusMessage);

            await sender.CloseAsync();
        }
    }
}

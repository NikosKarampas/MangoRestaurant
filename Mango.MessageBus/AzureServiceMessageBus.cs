using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.ReferenceHandler = ReferenceHandler.Preserve;            

            var jsonMessage = JsonSerializer.Serialize(message, message.GetType(), options);
            var serviceBusMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage));

            await sender.SendMessageAsync(serviceBusMessage);

            await sender.CloseAsync();
        }
    }
}

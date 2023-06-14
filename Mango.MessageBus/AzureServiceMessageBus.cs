using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mango.MessageBus
{
    public class AzureServiceMessageBus : IMessageBus
    {        
        private readonly ServiceBusClient _serviceBusClient;        

        public AzureServiceMessageBus(IConfiguration config)
        {            
            _serviceBusClient = new ServiceBusClient(config["AzureServiceBus:ConnectionString"]);            
        }

        public async Task PublishMessage(BaseMessage message, string queueOrTopicName)
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender(queueOrTopicName);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.ReferenceHandler = ReferenceHandler.Preserve;            

            var jsonMessage = JsonSerializer.Serialize(message, message.GetType(), options);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage);

            await sender.SendMessageAsync(serviceBusMessage);

            await sender.DisposeAsync();
        }
    }
}

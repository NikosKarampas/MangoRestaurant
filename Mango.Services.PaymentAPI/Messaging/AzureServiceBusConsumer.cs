using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.PaymentAPI.Messages;
using PaymentProcessor;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionPayment;        
        private readonly string orderPaymentProcessTopic;
        private readonly string orderupdatepaymentresulttopic;
        
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly IProcessPayment _processPayment;

        private ServiceBusProcessor orderPaymentProcessor;

        public AzureServiceBusConsumer(IConfiguration configuration, IMessageBus messageBus, IProcessPayment processPayment)
        {            
            _configuration = configuration;
            _messageBus = messageBus;
            _processPayment = processPayment;

            serviceBusConnectionString = _configuration["AzureServiceBus:ConnectionString"];
            subscriptionPayment = _configuration["AzureServiceBus:OrderPaymentSubscription"];
            orderPaymentProcessTopic = _configuration["AzureServiceBus:OrderPaymentTopicName"];
            orderupdatepaymentresulttopic = _configuration["AzureServiceBus:OrderUpdatePaymentResultTopic"];

            var client = new ServiceBusClient(serviceBusConnectionString);
            orderPaymentProcessor = client.CreateProcessor(orderPaymentProcessTopic, subscriptionPayment);            
        }

        public async Task Start()
        {
            orderPaymentProcessor.ProcessMessageAsync += ProcessPayments;
            orderPaymentProcessor.ProcessErrorAsync += ErrorHandler;
            await orderPaymentProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {            
            await orderPaymentProcessor.StopProcessingAsync();
            await orderPaymentProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task ProcessPayments(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.ReferenceHandler = ReferenceHandler.Preserve;

            PaymentRequestMessage paymentRequestMessage = JsonSerializer.Deserialize<PaymentRequestMessage>(body, options);

            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new UpdatePaymentResultMessage()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId
            };

            updatePaymentResultMessage.Id = Guid.NewGuid();
            updatePaymentResultMessage.MessageCreated = DateTime.Now;

            try
            {
                await _messageBus.PublishMessage(updatePaymentResultMessage, orderupdatepaymentresulttopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}

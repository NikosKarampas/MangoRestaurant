using Azure.Messaging.ServiceBus;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Repository;
using System.Text.Json;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionEmail;        
        private readonly string orderUpdatePaymentResultTopic;

        private readonly EmailRepository _emailRepository;        
        private readonly IConfiguration _configuration;        

        private ServiceBusProcessor checkoutProcessor;
        private ServiceBusProcessor orderUpdatePaymentStatusProcessor;

        public AzureServiceBusConsumer(EmailRepository emailRepository, IConfiguration configuration)
        {
            _emailRepository = emailRepository;
            _configuration = configuration;        

            serviceBusConnectionString = _configuration["AzureServiceBus:ConnectionString"];
            subscriptionEmail = _configuration["AzureServiceBus:SubcriptionName"];
            orderUpdatePaymentResultTopic = _configuration["AzureServiceBus:OrderUpdatePaymentResultTopic"];

            var client = new ServiceBusClient(serviceBusConnectionString);     
            orderUpdatePaymentStatusProcessor = client.CreateProcessor(orderUpdatePaymentResultTopic, subscriptionEmail);
        }

        public async Task Start()
        {          
            orderUpdatePaymentStatusProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            orderUpdatePaymentStatusProcessor.ProcessErrorAsync += ErrorHandler;
            await orderUpdatePaymentStatusProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {                        
            await orderUpdatePaymentStatusProcessor.StopProcessingAsync();
            await orderUpdatePaymentStatusProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }        

        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();

            //JsonSerializerOptions options = new JsonSerializerOptions();
            //options.ReferenceHandler = ReferenceHandler.Preserve;

            UpdatePaymentResultMessage updatePaymentResultMessage = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(body);

            try
            {
                await _emailRepository.SendAndLogEmail(updatePaymentResultMessage);

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

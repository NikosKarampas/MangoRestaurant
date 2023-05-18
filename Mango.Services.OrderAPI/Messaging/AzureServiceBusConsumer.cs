using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using System.Text;
using System.Text.Json;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionCheckout;
        private readonly string checkoutMessageTopic;

        private readonly OrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor checkoutProcessor;

        public AzureServiceBusConsumer(OrderRepository orderRepository, IMapper mapper, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _configuration = configuration;

            serviceBusConnectionString = configuration["AzureServiceBus:ConnectionString"];
            subscriptionCheckout = configuration["AzureServiceBus:SubscriptionCheckout"];
            checkoutMessageTopic = configuration["AzureServiceBus:CheckoutTopicName"];

            var client = new ServiceBusClient(serviceBusConnectionString);
            checkoutProcessor = client.CreateProcessor(checkoutMessageTopic, subscriptionCheckout);
        }

        public async Task Start()
        {
            checkoutProcessor.ProcessMessageAsync += OnCheckoutMessageReceived;
            checkoutProcessor.ProcessErrorAsync += ErrorHandler;
            await checkoutProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {            
            await checkoutProcessor.StopProcessingAsync();
            await checkoutProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnCheckoutMessageReceived(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();            

            CheckoutHeaderDto checkoutHeaderDto = JsonSerializer.Deserialize<CheckoutHeaderDto>(body);

            OrderHeader orderHeader = _mapper.Map<OrderHeader>(checkoutHeaderDto);
            orderHeader.PaymentStatus = false;
            orderHeader.OrderTime = DateTime.Now;
            orderHeader.CartTotalItems = orderHeader.OrderDetails.Count;

            await _orderRepository.AddOrderAsync(orderHeader);
        }
    }
}

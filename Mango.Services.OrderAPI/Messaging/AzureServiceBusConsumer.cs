using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer : IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionCheckout;
        //private readonly string checkoutMessageTopic;
        private readonly string checkoutQueueName;
        private readonly string orderPaymentProcessTopic;
        private readonly string orderUpdatePaymentResultTopic;

        private readonly OrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;

        private ServiceBusProcessor checkoutProcessor;
        private ServiceBusProcessor orderUpdatePaymentStatusProcessor;

        public AzureServiceBusConsumer(OrderRepository orderRepository, IMapper mapper, 
            IConfiguration configuration, IMessageBus messageBus)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _configuration = configuration;
            _messageBus = messageBus;

            serviceBusConnectionString = _configuration["AzureServiceBus:ConnectionString"];
            subscriptionCheckout = _configuration["AzureServiceBus:SubscriptionCheckout"];
            //checkoutMessageTopic = _configuration["AzureServiceBus:CheckoutTopicName"];
            checkoutQueueName = _configuration["AzureServiceBus:CheckoutQueueName"];
            orderPaymentProcessTopic = _configuration["AzureServiceBus:OrderPaymentTopicName"];
            orderUpdatePaymentResultTopic = _configuration["AzureServiceBus:OrderUpdatePaymentResultTopic"];

            var client = new ServiceBusClient(serviceBusConnectionString);
            //checkoutProcessor = client.CreateProcessor(checkoutMessageTopic, subscriptionCheckout);
            checkoutProcessor = client.CreateProcessor(checkoutQueueName);
            orderUpdatePaymentStatusProcessor = client.CreateProcessor(orderUpdatePaymentResultTopic, subscriptionCheckout);
        }

        public async Task Start()
        {
            checkoutProcessor.ProcessMessageAsync += OnCheckoutMessageReceived;
            checkoutProcessor.ProcessErrorAsync += ErrorHandler;
            await checkoutProcessor.StartProcessingAsync();

            orderUpdatePaymentStatusProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            orderUpdatePaymentStatusProcessor.ProcessErrorAsync += ErrorHandler;
            await orderUpdatePaymentStatusProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {            
            await checkoutProcessor.StopProcessingAsync();
            await checkoutProcessor.DisposeAsync();

            await orderUpdatePaymentStatusProcessor.StopProcessingAsync();
            await orderUpdatePaymentStatusProcessor.DisposeAsync();
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnCheckoutMessageReceived(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.ReferenceHandler = ReferenceHandler.Preserve;

            CheckoutHeaderDto checkoutHeaderDto = JsonSerializer.Deserialize<CheckoutHeaderDto>(body, options);

            OrderHeader orderHeader = _mapper.Map<OrderHeader>(checkoutHeaderDto);
            orderHeader.PaymentStatus = false;
            orderHeader.OrderTime = DateTime.Now;
            orderHeader.CartTotalItems = orderHeader.OrderDetails.ToList().Count;

            await _orderRepository.AddOrderAsync(orderHeader);

            PaymentRequestMessage paymentRequestMessage = new()
            {
                Name = orderHeader.FirstName + " " + orderHeader.LastName,
                CardNumber = orderHeader.CardNumber,
                CVV = orderHeader.CVV,
                ExpiryMonthYear = orderHeader.ExpiryMonthYear,
                OrderId = orderHeader.OrderHeaderId,
                OrderTotal = orderHeader.OrderTotal,
                Email = orderHeader.Email
            };

            paymentRequestMessage.Id = Guid.NewGuid();
            paymentRequestMessage.MessageCreated = DateTime.Now;

            try
            {
                await _messageBus.PublishMessage(paymentRequestMessage, orderPaymentProcessTopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();

            //JsonSerializerOptions options = new JsonSerializerOptions();
            //options.ReferenceHandler = ReferenceHandler.Preserve;

            UpdatePaymentResultMessage paymentResultMessage = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(body);

            await _orderRepository.UpdateOrderPaymentStatusAsync(paymentResultMessage.OrderId, paymentResultMessage.Status);

            await args.CompleteMessageAsync(args.Message);
        }
    }
}

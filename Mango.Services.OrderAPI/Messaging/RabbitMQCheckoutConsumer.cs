using Mango.Services.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json.Serialization;
using System.Text.Json;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using AutoMapper;

namespace Mango.Services.OrderAPI.Messaging
{
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        private IConnection _connection;
        private IModel _channel;

        public RabbitMQCheckoutConsumer(OrderRepository orderRepository, 
            IConfiguration config, 
            IMapper mapper)
        {
            _orderRepository = orderRepository;

            _config = config;
            _mapper = mapper;            

            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Hostname"],
                UserName = _config["RabbitMQ:Username"],
                Password = _config["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_config["RabbitMQ:CheckoutQueueName"], false, false, false, arguments: null);            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = ea.Body.ToString();

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.ReferenceHandler = ReferenceHandler.Preserve;

                CheckoutHeaderDto checkoutHeaderDto = JsonSerializer.Deserialize<CheckoutHeaderDto>(content, options);

                HandlerMessage(checkoutHeaderDto).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(_config["RabbitMQ:CheckoutQueueName"], false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandlerMessage(CheckoutHeaderDto checkoutHeaderDto)
        {
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
                //TODO
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}

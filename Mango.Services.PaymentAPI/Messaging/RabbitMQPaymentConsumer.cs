using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Mango.Services.PaymentAPI.RabbitMQSender;
using Mango.Services.PaymentAPI.Messages;
using PaymentProcessor;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {        
        private readonly IRabbitMQUpdatePaymentMessageSender _rabbitMQUpdatePaymentMessageSender;
        private readonly IProcessPayment _processPayment;
        private readonly IConfiguration _config;        

        private IConnection _connection;
        private IModel _channel;

        public RabbitMQPaymentConsumer(
            IRabbitMQUpdatePaymentMessageSender rabbitMQUpdatePaymentMessageSender,
            IProcessPayment processPayment,
            IConfiguration config)
        {
            _rabbitMQUpdatePaymentMessageSender = rabbitMQUpdatePaymentMessageSender;
            _processPayment = processPayment;
            _config = config;

            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Hostname"],
                UserName = _config["RabbitMQ:Username"],
                Password = _config["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_config["RabbitMQ:OrderPaymentQueueName"], false, false, false, arguments: null);            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.ReferenceHandler = ReferenceHandler.Preserve;                

                PaymentRequestMessage paymentRequestMessage = JsonSerializer.Deserialize<PaymentRequestMessage>(content, options);

                HandlerMessage(paymentRequestMessage).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(_config["RabbitMQ:OrderPaymentQueueName"], false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandlerMessage(PaymentRequestMessage paymentRequestMessage)
        {
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new UpdatePaymentResultMessage()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId,
                Email = paymentRequestMessage.Email
            };

            updatePaymentResultMessage.Id = Guid.NewGuid();
            updatePaymentResultMessage.MessageCreated = DateTime.Now;

            try
            {
                _rabbitMQUpdatePaymentMessageSender.SendMessage(updatePaymentResultMessage, _config["RabbitMQ:UpdatePaymentFanoutExchangeName"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}

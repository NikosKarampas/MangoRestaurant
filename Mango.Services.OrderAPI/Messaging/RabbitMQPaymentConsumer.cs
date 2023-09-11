using System.Text.Json;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Mango.Services.OrderAPI.Repository;
using Mango.Services.OrderAPI.Messages;

namespace Mango.Services.OrderAPI.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {                
        private readonly IConfiguration _config;        

        private IConnection _connection;
        private IModel _channel;

        private readonly string ExchangeName = string.Empty;

        private readonly OrderRepository _orderRepository;

        string QueueName = string.Empty;

        public RabbitMQPaymentConsumer(
            OrderRepository orderRepository,
            IConfiguration config)
        {
            _orderRepository = orderRepository;
            _config = config;

            ExchangeName = _config["RabbitMQ:UpdatePaymentFanoutExchangeName"];

            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Hostname"],
                UserName = _config["RabbitMQ:Username"],
                Password = _config["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
            QueueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(QueueName, ExchangeName, "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {                
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());          

                UpdatePaymentResultMessage updatePaymentResultMessage = JsonSerializer.Deserialize<UpdatePaymentResultMessage>(body);                

                HandlerMessage(updatePaymentResultMessage).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(QueueName, false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandlerMessage(UpdatePaymentResultMessage updatePaymentResultMessage)
        {            
            try
            {
                await _orderRepository.UpdateOrderPaymentStatusAsync(updatePaymentResultMessage.OrderId, updatePaymentResultMessage.Status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}

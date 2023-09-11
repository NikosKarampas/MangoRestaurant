﻿using Mango.MessageBus;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mango.Services.PaymentAPI.RabbitMQSender
{
    public class RabbitMQUpdatePaymentMessageSender : IRabbitMQUpdatePaymentMessageSender
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        
        private IConnection _connection;

        //private const string UpdatePaymentEmailQueueName = "updatepaymentemail";
        //private const string UpdatePaymentOrderQueueName = "updatepaymentorder";

        public RabbitMQUpdatePaymentMessageSender(string hostname, string username, string password)
        {
            _hostname = hostname;
            _username = username;
            _password = password;            
        }

        public void SendMessage(BaseMessage message, string fanoutExchangeName)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();

                //Declare fanout exchange
                channel.ExchangeDeclare(fanoutExchangeName, ExchangeType.Fanout);

                JsonSerializerOptions options = new JsonSerializerOptions();
                options.ReferenceHandler = ReferenceHandler.Preserve;

                var jsonMessage = JsonSerializer.Serialize(message, message.GetType(), options);
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                channel.BasicPublish(exchange: fanoutExchangeName, string.Empty, basicProperties: null, body: body);

                //Or alternative use direct exchange
                //channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: false);
                //channel.QueueDeclare(UpdatePaymentEmailQueueName, false, false, false, null);
                //channel.QueueDeclare(UpdatePaymentOrderQueueName, false, false, false, null);

                //channel.QueueBind(UpdatePaymentEmailQueueName, ExchangeName, "paymentemail");
                //channel.QueueBind(UpdatePaymentOrderQueueName, ExchangeName, "paymentorder");

                //channel.BasicPublish(exchange: ExchangeName, "paymentemail", basicProperties: null, body: body);
                //channel.BasicPublish(exchange: ExchangeName, "paymentorder", basicProperties: null, body: body);
            }            
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };

                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        private bool ConnectionExists()
        {
            if ( _connection != null )
            {
                return true;
            }
            CreateConnection();
            return _connection != null;
        }
    }
}

using Mango.MessageBus;
using Mango.Services.PaymentAPI.Extensions;
using Mango.Services.PaymentAPI.Messaging;
using Mango.Services.PaymentAPI.RabbitMQSender;
using PaymentProcessor;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// Add services to the container.

builder.Services.AddSingleton<IProcessPayment, ProcessPayment>();

builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();

builder.Services.AddSingleton<IMessageBus, AzureServiceMessageBus>();

builder.Services.AddHostedService<RabbitMQPaymentConsumer>();

builder.Services.AddSingleton<IRabbitMQUpdatePaymentMessageSender>(_ =>
    new RabbitMQUpdatePaymentMessageSender(
        config["RabbitMQ:Hostname"],
        config["RabbitMQ:Username"],
        config["RabbitMQ:Password"]));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseAzureServiceBusConsumer();

app.Run();

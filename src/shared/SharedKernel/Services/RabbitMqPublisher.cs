using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using SharedKernel.Interfaces;
using System.Text;

namespace SharedKernel.Services;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IConfiguration _config;
    public RabbitMqPublisher(IConfiguration config)
    {
        _config = config;
    }

    public async Task Publish(string queueName, string message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _config["RabbitMq:Host"],
            Port = int.Parse(_config["RabbitMq:Port"]),
            UserName = _config["RabbitMq:Username"],
            Password = _config["RabbitMq:Password"]
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();


        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
    }
}

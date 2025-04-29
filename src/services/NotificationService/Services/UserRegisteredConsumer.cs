using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService.Services;

public class UserRegisteredConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private IConnection _connection;
    private IChannel _channel;

    public UserRegisteredConsumer(IConfiguration config)
    {
        _config = config;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMq:Host"],
            Port = int.Parse(_config["RabbitMq:Port"]),
            UserName = _config["RabbitMq:Username"],
            Password = _config["RabbitMq:Password"]
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue: _config["RabbitMq:QueueName"],
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
        
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"📩 Received message: {message}");

            // TODO: Parse message and trigger email/notification logic
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(
            queue: _config["RabbitMq:QueueName"],
            autoAck: true,
            consumer: consumer
        );
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}

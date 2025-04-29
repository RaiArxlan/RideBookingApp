namespace SharedKernel.Interfaces;

public interface IRabbitMqPublisher
{
    Task Publish(string queueName, string message);
}

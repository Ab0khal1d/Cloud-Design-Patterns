using System.Text;
using AboKhaledQueue.Constants;
using Azure.Messaging.ServiceBus;

namespace AboKhaledQueue.Consumer;


public class QueueSessionConsumer(ILogger<QueueSessionConsumer> logger) : BackgroundService
{
    private readonly string _connectionString = AboKhaledServiceBusConstants.ConnectionString;
    private readonly string _queueName = AboKhaledServiceBusConstants.PaymentQueueName;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var client = new ServiceBusClient(_connectionString);
        // AcceptSessionAsync or AcceptNextSessionAsync method must be called for a queue has requiresSession set to true
        // if the queue has requiresSession set to false, it will throw an exception
        // ServiceBusSessionReceiver? receiver =await client.AcceptNextSessionAsync(_queueName, cancellationToken: stoppingToken);
        var receiver = await client.AcceptSessionAsync(_queueName, "session-1", cancellationToken: stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            var message = await receiver.ReceiveMessageAsync(cancellationToken: stoppingToken);
            if (message != null)
            {
                logger.LogInformation($"Received message: {Encoding.UTF8.GetString(message.Body)}");
                await receiver.CompleteMessageAsync(message, stoppingToken);
            }else
            {
                logger.LogInformation("No messages available");
            }
        }
    }
    
}
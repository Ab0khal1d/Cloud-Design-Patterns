using System.Text;
using AboKhaledQueue.Constants;
using Azure.Messaging.ServiceBus;

namespace AboKhaledQueue.Consumer;

public class QueueConsumer(ILogger<QueueConsumer> logger) : BackgroundService
{
    private readonly string _connectionString = AboKhaledServiceBusConstants.ConnectionString;
    private readonly string _queueName = AboKhaledServiceBusConstants.PaymentQueueName;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var client = new ServiceBusClient(_connectionString);
        ServiceBusReceiver receiver = client.CreateReceiver(_queueName);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
            
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
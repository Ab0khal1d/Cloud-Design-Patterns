using System.Text;
using AboKhaledQueue.Constants;
using Azure.Messaging.ServiceBus;

namespace AboKhaledQueue.Consumer;

public class QueueConsumerProcessor(ILogger<QueueConsumerProcessor> logger) : BackgroundService
{
    private readonly string _queueName = AboKhaledServiceBusConstants.PaymentQueueName;
    private readonly ServiceBusClient _client = new ServiceBusClient(AboKhaledServiceBusConstants.ConnectionString);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = _client.CreateProcessor(_queueName);
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;
        
        await processor.StartProcessingAsync(stoppingToken);
    }

    private Task MessageHandler (ProcessMessageEventArgs args)
    {
        var message = args.Message;
        logger.LogInformation($"Received message: {Encoding.UTF8.GetString(message.Body)}");
        // no need to complete the message, it will be done automatically
        // await args.CompleteMessageAsync(message);
        // if execution occur, message won't be completed
        // throw new Exception("Error occurred");
        return Task.CompletedTask;
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Error occurred");
        return Task.CompletedTask;
    }
}
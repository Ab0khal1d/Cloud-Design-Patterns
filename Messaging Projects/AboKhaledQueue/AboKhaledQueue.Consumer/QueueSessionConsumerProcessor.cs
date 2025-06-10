using System.Text;
using AboKhaledQueue.Constants;
using Azure.Messaging.ServiceBus;

namespace AboKhaledQueue.Consumer;

public class QueueSessionConsumerProcessor(ILogger<QueueSessionConsumerProcessor> logger) : BackgroundService
{
    private readonly string _queueName = AboKhaledServiceBusConstants.PaymentQueueName;
    private readonly ServiceBusClient _client = new ServiceBusClient(AboKhaledServiceBusConstants.ConnectionString);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new ServiceBusSessionProcessorOptions
        {
            // By default after the message handler returns, the processor will complete the message
            // If I want more fine-grained control over settlement, I can set this to false.
            // AutoCompleteMessages = false,

            // I can also allow for processing multiple sessions
            MaxConcurrentSessions = 5,

            // By default or when AutoCompleteMessages is set to true, the processor will complete the message after executing the message handler
            // Set AutoCompleteMessages to false to [settle messages](https://docs.microsoft.com/en-us/azure/service-bus-messaging/message-transfers-locks-settlement#peeklock) on your own.
            // In both cases, if the message handler throws an exception without settling the message, the processor will abandon the message.
            MaxConcurrentCallsPerSession = 2,

            // Processing can be optionally limited to a subset of session Ids.
            SessionIds = { "Session-1" },
        };

        var processor = _client.CreateSessionProcessor(_queueName, options);
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        // configure optional event handlers that will be executed when a session starts processing and stops processing
        // NOTE: The SessionInitializingAsync event is raised when the processor obtains a lock for a session. This does not mean the session was
        // never processed before by this or any other ServiceBusSessionProcessor instances. Similarly, the SessionClosingAsync
        // event is raised when no more messages are available for the session being processed subject to the SessionIdleTimeout
        // in the ServiceBusSessionProcessorOptions. If additional messages are sent for that session later, the SessionInitializingAsync and SessionClosingAsync
        // events would be raised again.

        processor.SessionInitializingAsync += SessionInitializingHandler;
        processor.SessionClosingAsync += SessionClosingHandler;
        await processor.StartProcessingAsync(stoppingToken);
    }

    public Task MessageHandler(ProcessSessionMessageEventArgs args)
    {
        var message = args.Message;
        logger.LogInformation($"Received message: {Encoding.UTF8.GetString(message.Body)}");
        // no need to complete the message, it will be done automatically
        // await args.CompleteMessageAsync(message);
        // if execution occur, message won't be completed
        // throw new Exception("Error occurred");
        return Task.CompletedTask;
    }

    public Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Error occurred");
        return Task.CompletedTask;
    }

    async Task SessionInitializingHandler(ProcessSessionEventArgs args)
    {
        await args.SetSessionStateAsync(
            new BinaryData("Some state specific to this session when the session is opened for processing."));
    }

    async Task SessionClosingHandler(ProcessSessionEventArgs args)
    {
        // We may want to clear the session state when no more messages are available for the session or when some known terminal message
        // has been received. This is entirely dependent on the application scenario.
        BinaryData sessionState = await args.GetSessionStateAsync();
        if (sessionState.ToString() ==
            "Some state that indicates the final message was received for the session")
        {
            await args.SetSessionStateAsync(null);
        }
    }
}
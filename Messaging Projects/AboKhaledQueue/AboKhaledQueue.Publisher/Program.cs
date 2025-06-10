using System.Text;
using AboKhaledQueue.Constants;
using Azure.Messaging.ServiceBus;

const string connectionString =AboKhaledServiceBusConstants.ConnectionString;
   
const string queueName = AboKhaledServiceBusConstants.PaymentQueueName;

await using var client = new ServiceBusClient(connectionString);
ServiceBusSender sender = client.CreateSender(queueName);

var message = new ServiceBusMessage("Hello world!"u8.ToArray());
await sender.SendMessageAsync(message);

var sessionMessage = new ServiceBusMessage("This is the first message should be in session-1"u8.ToArray())
{
    SessionId = "session-1"
};
await sender.SendMessageAsync(sessionMessage);
await sender.SendMessageAsync(message);

Console.WriteLine("Message sent successfully");
Console.ReadKey();

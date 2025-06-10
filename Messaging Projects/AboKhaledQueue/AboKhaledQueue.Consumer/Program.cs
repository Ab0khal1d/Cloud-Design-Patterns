using AboKhaledQueue.Consumer;

var builder = Host.CreateApplicationBuilder(args);
// builder.Services.AddHostedService<QueueConsumer>();
// builder.Services.AddHostedService<QueueConsumerProcessor>();
builder.Services.AddHostedService<QueueSessionConsumer>();
builder.Services.AddHostedService<QueueSessionConsumerProcessor>();

var host = builder.Build();
host.Run();
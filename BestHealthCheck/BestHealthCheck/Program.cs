using BestHealthCheck.Health;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<JsonPlaceHolderHealthCheck>("JsonPlaceHolderHealthCheck");
builder.Services.AddHttpClient("jsonplaceholder",
    c => { c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/"); });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthChecks("_health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/user", async (IHttpClientFactory httpClientFactory) =>
    {
        var client = httpClientFactory.CreateClient("jsonplaceholder");
        var response = await client.GetAsync("users");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Ok(content);
    })
    .WithName("GetUsers")
    .WithOpenApi();

app.Run();
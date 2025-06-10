using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

const string tmDbClient = "TMDb-Client";
const string tmDbUrl = "https://api.themoviedb.org/3";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var keyVaultUri = builder.Configuration["KeyVault:Vault"];
var clientId = builder.Configuration["KeyVault:ClientId"];
var clientSecret = builder.Configuration["KeyVault:ClientSecret"];
var tenantId = builder.Configuration["KeyVault:TenantId"];
var apiKeySecretName = builder.Configuration["KeyVault:TMDbApiKeySecretName"];

if (string.IsNullOrEmpty(keyVaultUri) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) ||
    string.IsNullOrEmpty(tenantId))
{
    throw new InvalidOperationException("KeyVault configuration is missing");
}

var secretClient = new SecretClient(new Uri(keyVaultUri), new ClientSecretCredential(tenantId, clientId, clientSecret));
builder.Configuration.AddAzureKeyVault(
    secretClient,
    new AzureKeyVaultConfigurationOptions()
    {
        ReloadInterval = TimeSpan.FromMinutes(30)
    }
);

var tmDbApiKey = secretClient.GetSecret(apiKeySecretName).Value.Value;

var tmDbToken = builder.Configuration["TMDb:AccessToken"];
builder.Services.AddHttpClient(tmDbClient, client =>
{
    client.BaseAddress = new Uri(tmDbUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tmDbToken}");
});

// want to retrieve all sources of configuration to know order of precedence
// last one wins
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#configuration-providers
// https://devblogs.microsoft.com/premier-developer/order-of-precedence-when-configuring-asp-net-core/
// order of configuration sources is 1. appsettings.json 2. appsettings.{Environment}.json 3. User secrets
// 4. Environment variables 5. Command-line arguments 6. Azure Key Vault
// when using Azure Key Vault, it is added as the last source of configuration and overrides all other sources
var sources = builder.Configuration.Sources;
foreach (var source in sources)
{
    Console.WriteLine(source.GetType().Name);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/box-office",async (IHttpClientFactory httpClientFactory,IConfiguration configuration) =>
    {
        var tmDbApiKey = configuration[apiKeySecretName];
        var client = httpClientFactory.CreateClient(tmDbClient);
        // client.DefaultRequestHeaders.Remove("Authorization");
        // client.DefaultRequestHeaders.Add("Authorization","Bearer "+tmDbApiKey);
        var response =await client.GetAsync("movie/now_playing?language=en-US&page=1");
        response.EnsureSuccessStatusCode();
        var content = response.Content.ReadAsStringAsync().Result;
        return content;
    })
    .WithName("BoxOfficeMovies")
    .WithOpenApi();

app.Run();

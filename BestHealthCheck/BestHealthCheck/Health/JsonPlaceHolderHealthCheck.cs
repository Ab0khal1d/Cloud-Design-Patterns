using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BestHealthCheck.Health;

public class JsonPlaceHolderHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public JsonPlaceHolderHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var client = _httpClientFactory.CreateClient("jsonplaceholder");
            var response = await client.GetAsync("users", cancellationToken);
            response.EnsureSuccessStatusCode();
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(e.Message);
        }
    }
}
using System.Net.Http.Json;
using JobDiscovery.Api.Models.Greenhouse;

namespace JobDiscovery.Api.Clients.Greenhouse;

public sealed class GreenhouseClient
{
    private readonly HttpClient _httpClient;

    public GreenhouseClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GreenhouseJobBoardResponse> GetJobsAsync(
        string boardToken,
        CancellationToken cancellationToken = default)
    {
        var response =
            await _httpClient.GetFromJsonAsync<GreenhouseJobBoardResponse>(
                $"{boardToken}/jobs",
                cancellationToken
            );

        return response ?? new GreenhouseJobBoardResponse();
    }
}
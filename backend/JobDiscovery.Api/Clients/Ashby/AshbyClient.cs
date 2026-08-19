using JobDiscovery.Api.Configuration;
using JobDiscovery.Api.Models.Ashby;
using Microsoft.Extensions.Options;

namespace JobDiscovery.Api.Clients.Ashby;

public sealed class AshbyClient
{
  private readonly HttpClient _httpClient;
  private readonly AshbyOptions _options;

  public AshbyClient(
    HttpClient httpClient,
    IOptions<AshbyOptions> options)
  {
    _httpClient = httpClient;
    _options = options.Value;
  }

  public async Task<AshbyJobBoardResponse> GetJobsAsync(
    CancellationToken cancellationToken = default)
  {
    var response =
      await _httpClient.GetFromJsonAsync<AshbyJobBoardResponse>(
        _options.CompanyIdentifier,
        cancellationToken
      );
      return response ?? new AshbyJobBoardResponse();
  }
}
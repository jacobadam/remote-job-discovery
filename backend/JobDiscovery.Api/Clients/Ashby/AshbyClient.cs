using JobDiscovery.Api.Models.Ashby;
namespace JobDiscovery.Api.Clients.Ashby;
public sealed class AshbyClient
{
  private readonly HttpClient _httpClient;
  public AshbyClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<AshbyJobBoardResponse> GetJobsAsync(
    string jobBoardName,
    CancellationToken cancellationToken = default)
  {
    var response =
      await _httpClient.GetFromJsonAsync<AshbyJobBoardResponse>(
        jobBoardName,
        cancellationToken
      );
      return response ?? new AshbyJobBoardResponse();
  }
}
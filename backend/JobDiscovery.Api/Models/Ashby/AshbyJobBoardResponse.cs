namespace JobDiscovery.Api.Models.Ashby;


public sealed class AshbyJobBoardResponse
{
  public string ApiVersion { get; init; } = string.Empty;
  
  public List<AshbyJob> Jobs {get; init;} = [];
}
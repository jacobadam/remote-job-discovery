namespace JobDiscovery.Api.Models.Jobs;
public sealed class JobListing
{
  public string Source { get; init; } = string.Empty;

  public string SourceJobId { get; init; } = string.Empty;

  public string CompanyName { get; init; } = string.Empty;

  public string Title { get; init; } = string.Empty; 

  public string Location { get; init; } = string.Empty;

  public string WorkPlaceType { get; init; } = string.Empty;

  public DateTimeOffset? PublishedAt { get; init; }

  public string JobUrl { get; init; } = string.Empty;

  public string ApplyUrl { get; init; } = string.Empty;
}

// Location, WorkPlaceType, Date
namespace JobDiscovery.Api.Models.Ashby;

public sealed class AshbyJob
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public bool? IsRemote { get; init; }
    public string WorkplaceType { get; init; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; init; }
    public bool IsListed { get; init; }
    public string JobUrl { get; init; } = string.Empty;
    public string ApplyUrl { get; init; } = string.Empty;
}
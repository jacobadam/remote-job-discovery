using System.Text.Json.Serialization;

namespace JobDiscovery.Api.Models.Greenhouse;

public sealed class GreenhouseJob
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("company_name")]
    public string CompanyName { get; init; } = string.Empty;

    public GreenhouseLocation Location { get; init; } = new();

    [JsonPropertyName("first_published")]
    public DateTimeOffset? FirstPublished { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }

    [JsonPropertyName("absolute_url")]
    public string AbsoluteUrl { get; init; } = string.Empty;
}

public sealed class GreenhouseLocation
{
    public string Name { get; init; } = string.Empty;
}
namespace JobDiscovery.Api.Configuration;

public sealed class GreenhouseOptions
{
    public const string SectionName = "Greenhouse";

    public string BaseUrl { get; init; } = string.Empty;

    public List<GreenhouseCompanyOptions> Companies { get; init; } = [];
}

public sealed class GreenhouseCompanyOptions
{
    public string Name { get; init; } = string.Empty;

    public string BoardToken { get; init; } = string.Empty;
}
namespace JobDiscovery.Api.Configuration;

public sealed class AshbyOptions
{
  public const string SectionName = "Ashby";

  public string BaseUrl { get; init; } = string.Empty;

  public string CompanyIdentifier { get; init; } = string.Empty;
}
using System.Text.Json;
using JobDiscovery.Api.Clients.Ashby;
using JobDiscovery.Api.Configuration;
using JobDiscovery.Api.Models.Jobs;
using Microsoft.Extensions.Options;

namespace JobDiscovery.Api.Services.Ashby;

public sealed class AshbyJobService
{
  private readonly AshbyClient _ashbyClient;
  private readonly AshbyOptions _options;
  private readonly ILogger<AshbyJobService> _logger;
  
  public AshbyJobService(
    AshbyClient ashbyClient,
    IOptions<AshbyOptions> options,
    ILogger<AshbyJobService> logger)
  {
    _ashbyClient = ashbyClient;
    _options = options.Value;
    _logger = logger;
  }

  public async Task<IReadOnlyList<JobListing>> GetRemoteJobsAsync(
      CancellationToken cancellationToken = default)
  {
      var jobListings = new List<JobListing>();

      foreach (var company in _options.Companies)
    {
      try
      {
        var response = await _ashbyClient.GetJobsAsync(
          company.JobBoardName,
          cancellationToken
        );

        var companyJobs = response.Jobs
          .Where(job => 
            job.IsListed && 
            string.Equals(
              job.WorkplaceType,
              "Remote",
              StringComparison.OrdinalIgnoreCase
            )
          )
          .Select(job => new JobListing
          {
            Source = "Ashby",
            SourceJobId = job.Id,
            CompanyName = company.Name,
            Title = job.Title.Trim(),
            Location = job.Location.Trim(),
            WorkplaceType = job.WorkplaceType,
            PublishedAt = job.PublishedAt,
            JobUrl = job.JobUrl,
            ApplyUrl = job.ApplyUrl
          });

        jobListings.AddRange(companyJobs);
      }
      catch (HttpRequestException exception)
      {
        _logger.LogWarning(
          exception,
          "Failed to request jobs for {CompanyName} using job board {JobBoardName}.",
          company.Name,
          company.JobBoardName
        );
      }
      catch (JsonException exception)
      {
        _logger.LogWarning(
          exception,
          "Failed to read the Ashby response for {CompanyName}.",
          company.Name
        );
      }
    }
    return jobListings
        .OrderByDescending(job => job.PublishedAt)
        .ToList();
  }
}

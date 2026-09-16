using System.Text.Json;
using JobDiscovery.Api.Clients.Greenhouse;
using JobDiscovery.Api.Configuration;
using JobDiscovery.Api.Models.Jobs;
using Microsoft.Extensions.Options;

namespace JobDiscovery.Api.Services.Greenhouse;

public sealed class GreenhouseJobService
{
    private readonly GreenhouseClient _greenhouseClient;
    private readonly GreenhouseOptions _options;
    private readonly ILogger<GreenhouseJobService> _logger;

    public GreenhouseJobService(
      GreenhouseClient greenhouseClient,
      IOptions<GreenhouseOptions> options,
      ILogger<GreenhouseJobService> logger)
    {
        _greenhouseClient = greenhouseClient;
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
                var response = await _greenhouseClient.GetJobsAsync(
                  company.BoardToken,
                  cancellationToken
                );

                var companyJobs = response.Jobs
                  .Where(job =>
                    job.Location.Name.Contains(
                      "remote",
                      StringComparison.OrdinalIgnoreCase
                    )
                  )
                  .Select(job => new JobListing
                  {
                      Source = "Greenhouse",
                      SourceJobId = job.Id.ToString(),
                      CompanyName = company.Name,
                      Title = job.Title.Trim(),
                      Location = job.Location.Name.Trim(),
                      WorkplaceType = "Remote",
                      PublishedAt = job.FirstPublished,
                      JobUrl = job.AbsoluteUrl,
                      ApplyUrl = job.AbsoluteUrl
                  });

                jobListings.AddRange(companyJobs);
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to request jobs for {CompanyName} using board token {BoardToken}.",
                    company.Name,
                    company.BoardToken
                );
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(
                exception,
                "Failed to read the Greenhouse response for {CompanyName}.",
                company.Name
              );
            }
        }
        return jobListings
          .OrderByDescending(job => job.PublishedAt)
          .ToList();
    }
}
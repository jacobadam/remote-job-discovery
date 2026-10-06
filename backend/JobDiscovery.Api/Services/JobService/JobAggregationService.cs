namespace JobDiscovery.Api.Services;

public class JobAggregationService
{
    private readonly IEnumerable<IJobProvider> _jobProviders;

    public JobAggregationService(
      IEnumerable<IJobProvider> jobProviders
    )
    {
        _jobProviders = jobProviders;
    }

    public async Task<IReadOnlyList<JobListing>> GetJobsAsync(
      string? title,
      CancellationToken cancellationToken = default
    )
    {
        var jobTasks = _jobProviders
          .Select(provider =>
            provider.GetRemoteJobsAsync(cancellationToken)
          );

        var jobLists = await Task.WhenAll(jobTasks)


    var jobs = jobLists
        .SelectMany(jobList => jobList);

        if (!string.IsNullOrWhiteSpace(title))
        {
            jobs = jobs.Where(job =>
                job.Title.Contains(
                    title.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }
        return jobs
          .OrderByDescending(job => job.PublishedAt)
          .ToList();
    }
}
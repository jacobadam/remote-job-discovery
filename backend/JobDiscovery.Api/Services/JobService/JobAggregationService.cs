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
      CancellationToken cancellationToken = default
    )
    {
        var jobTasks = _jobProviders
          .Select(provider =>
            provider.GetRemoteJobsAsync(cancellationToken)
          );

        var jobList = await Task.WhenAll(jobTasks)
  
    return jobList
      .SelectMany(jobList => jobList)
      .ToList();
    }
}
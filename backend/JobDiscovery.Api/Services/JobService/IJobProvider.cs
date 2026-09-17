using JobDiscovery.Api.Models.Jobs;

namespace JobDiscovery.Api.Services;

public interface IJobProvider
{
  Task<IReadOnlyList<JobListing>> GetRemoteJobsAsync(
    CancellationToken cancellationToken = default
  );
}
using JobDiscovery.Api.Models.Jobs;
using JobDiscovery.Api.Models.Common;

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

    public async Task<PagedResult<JobListing>> GetJobsAsync(
      string? title,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default
    )
    {
        var jobTasks = _jobProviders
          .Select(provider =>
            provider.GetRemoteJobsAsync(cancellationToken)
          );

        var jobLists = await Task.WhenAll(jobTasks);


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
        var orderedJobs = jobs
          .OrderByDescending(job => job.PublishedAt)
          .ToList();

        var totalCount = orderedJobs.Count;

        var pagedJobs = orderedJobs
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

        var totalPages = (int)Math.Ceiling(
          totalCount / (double)pageSize
        );

        return new PagedResult<JobListing>
        {
            Items = pagedJobs,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
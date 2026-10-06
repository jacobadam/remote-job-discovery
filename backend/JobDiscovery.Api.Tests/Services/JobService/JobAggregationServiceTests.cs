using JobDiscovery.Api.Models.Jobs;
using JobDiscovery.Api.Services;

namespace JobDiscovery.Api.Tests.Services.JobService;

public class JobAggregationServiceTests
{
    private class FakeJobProvider : IJobProvider
    {
        private readonly IReadOnlyList<JobListing> _jobs;

        public FakeJobProvider(IReadOnlyList<JobListing> jobs)
        {
            _jobs = jobs;
        }

        public Task<IReadOnlyList<JobListing>> GetRemoteJobsAsync(
            CancellationToken cancellationToken = default
        )
        {
            return Task.FromResult(_jobs);
        }
    }

    [Fact]
    public async Task GetJobsAsync_CombinesJobsFromAllProviders()
    {
        var firstProvider = new FakeJobProvider(
            new List<JobListing>
            {
            new()
            {
                Source = "Ashby",
                SourceJobId = "1",
                CompanyName = "Company A",
                Title = "Software Engineer",
                PublishedAt = DateTimeOffset.Parse("2026-10-01")
            },
            new()
            {
                Source = "Ashby",
                SourceJobId = "2",
                CompanyName = "Company B",
                Title = "Frontend Engineer",
                PublishedAt = DateTimeOffset.Parse("2026-10-02")
            }
            }
        );

        var secondProvider = new FakeJobProvider(
            new List<JobListing>
            {
            new()
            {
                Source = "Greenhouse",
                SourceJobId = "3",
                CompanyName = "Company C",
                Title = "Backend Engineer",
                PublishedAt = DateTimeOffset.Parse("2026-10-03")
            }
            }
        );

        var service = new JobAggregationService(
            new IJobProvider[]
            {
            firstProvider,
            secondProvider
            }
        );

        var jobs = await service.GetJobsAsync(null);

        Assert.Equal(3, jobs.Count);
    }
}
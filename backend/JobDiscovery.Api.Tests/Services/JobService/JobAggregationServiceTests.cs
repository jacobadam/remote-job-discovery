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

        var result = await service.GetJobsAsync(
            null,
            1,
            20
        );

        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetJobsAsync_FiltersJobsByTitle()
    {
        var provider = new FakeJobProvider(
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
                    Title = "Product Designer",
                    PublishedAt = DateTimeOffset.Parse("2026-10-02")
                }
            }
        );

        var service = new JobAggregationService(
            new IJobProvider[]
            {
                provider
            }
        );

        var result = await service.GetJobsAsync(
            "software",
            1,
            20
        );

        Assert.Single(result.Items);
        Assert.Equal("Software Engineer", result.Items[0].Title);
    }

    [Fact]
    public async Task GetJobsAsync_OrdersJobsByPublishedAtDescending()
    {
        var provider = new FakeJobProvider(
            new List<JobListing>
            {
                new()
                {
                    Source = "Ashby",
                    SourceJobId = "1",
                    CompanyName = "Company A",
                    Title = "Older Job",
                    PublishedAt = DateTimeOffset.Parse("2026-10-01")
                },
                new()
                {
                    Source = "Ashby",
                    SourceJobId = "2",
                    CompanyName = "Company B",
                    Title = "Newest Job",
                    PublishedAt = DateTimeOffset.Parse("2026-10-03")
                },
                new()
                {
                    Source = "Ashby",
                    SourceJobId = "3",
                    CompanyName = "Company C",
                    Title = "Middle Job",
                    PublishedAt = DateTimeOffset.Parse("2026-10-02")
                }
            }
        );

        var service = new JobAggregationService(
            new IJobProvider[]
            {
                provider
            }
        );

        var result = await service.GetJobsAsync(
            null,
            1,
            20
        );

        Assert.Equal("Newest Job", result.Items[0].Title);
        Assert.Equal("Middle Job", result.Items[1].Title);
        Assert.Equal("Older Job", result.Items[2].Title);
    }

    [Fact]
    public async Task GetJobsAsync_TrimsTitleFilterWhitespace()
    {
        var provider = new FakeJobProvider(
            new List<JobListing>
            {
                new()
                {
                    Source = "Greenhouse",
                    SourceJobId = "1",
                    CompanyName = "Company A",
                    Title = "Software Engineer",
                    PublishedAt = DateTimeOffset.Parse("2026-10-01")
                },
                new()
                {
                    Source = "Greenhouse",
                    SourceJobId = "2",
                    CompanyName = "Company B",
                    Title = "Product Designer",
                    PublishedAt = DateTimeOffset.Parse("2026-10-02")
                }
            }
        );

        var service = new JobAggregationService(
            new IJobProvider[]
            {
                provider
            }
        );

        var result = await service.GetJobsAsync(
            "  software  ",
            1,
            20
        );

        Assert.Single(result.Items);
        Assert.Equal("Software Engineer", result.Items[0].Title);
    }
}
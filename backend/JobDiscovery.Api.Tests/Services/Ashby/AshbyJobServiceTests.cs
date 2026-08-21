using System.Net;
using System.Text;
using JobDiscovery.Api.Clients.Ashby;
using JobDiscovery.Api.Configuration;
using JobDiscovery.Api.Services.Ashby;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JobDiscovery.Api.Tests.Services.Ashby;

public sealed class AshbyJobServiceTests
{
    [Fact]
    public async Task GetRemoteJobsAsync_ReturnsOnlyListedRemoteJobs()
    {
        const string json = """
        {
          "apiVersion": "1",
          "jobs": [
            {
              "id": "remote-job",
              "title": "Software Engineer",
              "location": "Remote - UK",
              "isRemote": true,
              "workplaceType": "Remote",
              "publishedAt": "2026-08-10T10:00:00+00:00",
              "isListed": true,
              "jobUrl": "https://example.com/remote-job",
              "applyUrl": "https://example.com/remote-job/apply"
            },
            {
              "id": "hybrid-job",
              "title": "Hybrid Engineer",
              "location": "London",
              "isRemote": true,
              "workplaceType": "Hybrid",
              "publishedAt": "2026-08-11T10:00:00+00:00",
              "isListed": true,
              "jobUrl": "https://example.com/hybrid-job",
              "applyUrl": "https://example.com/hybrid-job/apply"
            },
            {
              "id": "unlisted-job",
              "title": "Unlisted Engineer",
              "location": "Remote",
              "isRemote": true,
              "workplaceType": "Remote",
              "publishedAt": "2026-08-12T10:00:00+00:00",
              "isListed": false,
              "jobUrl": "https://example.com/unlisted-job",
              "applyUrl": "https://example.com/unlisted-job/apply"
            }
          ]
        }
        """;

        var messageHandler = new StubHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                )
            }
        );

        var httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri(
                "https://api.ashbyhq.com/posting-api/job-board/"
            )
        };

        var ashbyClient = new AshbyClient(httpClient);

        var options = Options.Create(
            new AshbyOptions
            {
                Companies =
                [
                    new AshbyCompanyOptions
                    {
                        Name = "Test Company",
                        JobBoardName = "test-company"
                    }
                ]
            }
        );

        var service = new AshbyJobService(
            ashbyClient,
            options,
            NullLogger<AshbyJobService>.Instance
        );

        var jobs = await service.GetRemoteJobsAsync();

        var job = Assert.Single(jobs);

        Assert.Equal("remote-job", job.SourceJobId);
        Assert.Equal("Test Company", job.CompanyName);
        Assert.Equal("Software Engineer", job.Title);
        Assert.Equal("Remote - UK", job.Location);
        Assert.Equal("Ashby", job.Source);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
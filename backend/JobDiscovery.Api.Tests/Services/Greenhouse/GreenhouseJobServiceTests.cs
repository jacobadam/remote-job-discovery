using System.Net;
using System.Text;
using JobDiscovery.Api.Clients.Greenhouse;
using JobDiscovery.Api.Configuration;
using JobDiscovery.Api.Services.Greenhouse;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JobDiscovery.Api.Tests.Services.Greenhouse;

public sealed class GreenhouseJobServiceTests
{
    [Fact]
    public async Task GetRemoteJobsAsync_ReturnsOnlyRemoteJobs()
    {
        const string json = """
      {
        "jobs": [
          {
            "id": 8711638002,
            "title": "Remote Software Engineer",
            "company_name": "Monzo",
            "location": {
              "name": "Cardiff, London or Remote (UK)"
            },
            "first_published": "2026-08-10T10:00:00+00:00",
            "updated_at": "2026-08-11T10:00:00+00:00",
            "absolute_url": "https://example.com/remote-job"
          },
          {
            "id": 1002,
            "title": "Office Software Engineer",
            "company_name": "Monzo",
            "location": {
              "name": "London"
            },
            "first_published": "2026-08-11T10:00:00+00:00",
            "updated_at": "2026-08-12T10:00:00+00:00",
            "absolute_url": "https://example.com/office-job"
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
                "https://boards-api.greenhouse.io/v1/boards/"
            )
        };

        var greenhouseClient = new GreenhouseClient(httpClient);

        var options = Options.Create(
            new GreenhouseOptions
            {
                Companies =
                [
                    new GreenhouseCompanyOptions
                  {
                      Name = "Monzo",
                      BoardToken = "monzo"
                  }
                ]
            }
        );

        var service = new GreenhouseJobService(
            greenhouseClient,
            options,
            NullLogger<GreenhouseJobService>.Instance
        );

        var jobs = await service.GetRemoteJobsAsync();

        var job = Assert.Single(jobs);

        Assert.Equal("8711638002", job.SourceJobId);
        Assert.Equal("Monzo", job.CompanyName);
        Assert.Equal("Remote Software Engineer", job.Title);
        Assert.Equal(
            "Cardiff, London or Remote (UK)",
            job.Location
        );
        Assert.Equal("Remote", job.WorkplaceType);
        Assert.Equal("Greenhouse", job.Source);
        Assert.Equal(
            "https://example.com/remote-job",
            job.JobUrl
        );
        Assert.Equal(job.JobUrl, job.ApplyUrl);
    }

    [Fact]
    public async Task GetRemoteJobsAsync_OrdersJobsByPublishedDateDescending()
    {
        const string json = """
    {
      "jobs": [
        {
          "id": 1001,
          "title": "Older Job",
          "company_name": "Monzo",
          "location": {
            "name": "Remote - UK"
          },
          "first_published": "2026-07-01T10:00:00+00:00",
          "updated_at": "2026-07-01T10:00:00+00:00",
          "absolute_url": "https://example.com/older-job"
        },
        {
          "id": 1002,
          "title": "Newer Job",
          "company_name": "Monzo",
          "location": {
            "name": "Remote - UK"
          },
          "first_published": "2026-08-01T10:00:00+00:00",
          "updated_at": "2026-08-01T10:00:00+00:00",
          "absolute_url": "https://example.com/newer-job"
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
                "https://boards-api.greenhouse.io/v1/boards/"
            )
        };

        var greenhouseClient = new GreenhouseClient(httpClient);

        var options = Options.Create(
            new GreenhouseOptions
            {
                Companies =
                [
                    new GreenhouseCompanyOptions
                {
                    Name = "Monzo",
                    BoardToken = "monzo"
                }
                ]
            }
        );

        var service = new GreenhouseJobService(
            greenhouseClient,
            options,
            NullLogger<GreenhouseJobService>.Instance
        );

        var jobs = await service.GetRemoteJobsAsync();

        Assert.Equal(2, jobs.Count);
        Assert.Equal("1002", jobs[0].SourceJobId);
        Assert.Equal("1001", jobs[1].SourceJobId);
    }

    [Fact]
    public async Task GetRemoteJobsAsync_ContinuesWhenOneCompanyRequestFails()
    {
        const string successfulJson = """
    {
      "jobs": [
        {
          "id": 1001,
          "title": "Software Engineer",
          "company_name": "Successful Company",
          "location": {
            "name": "Remote - UK"
          },
          "first_published": "2026-08-10T10:00:00+00:00",
          "updated_at": "2026-08-11T10:00:00+00:00",
          "absolute_url": "https://example.com/successful-job"
        }
      ]
    }
    """;

        var messageHandler = new StubHttpMessageHandler(request =>
        {
            var path = request.RequestUri?.AbsolutePath;

            if (path?.Contains(
                    "/failed-company/jobs",
                    StringComparison.OrdinalIgnoreCase
                ) == true)
            {
                return new HttpResponseMessage(
                    HttpStatusCode.InternalServerError
                );
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    successfulJson,
                    Encoding.UTF8,
                    "application/json"
                )
            };
        });

        var httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = new Uri(
                "https://boards-api.greenhouse.io/v1/boards/"
            )
        };

        var greenhouseClient = new GreenhouseClient(httpClient);

        var options = Options.Create(
            new GreenhouseOptions
            {
                Companies =
                [
                    new GreenhouseCompanyOptions
                {
                    Name = "Failed Company",
                    BoardToken = "failed-company"
                },
                new GreenhouseCompanyOptions
                {
                    Name = "Successful Company",
                    BoardToken = "successful-company"
                }
                ]
            }
        );

        var service = new GreenhouseJobService(
            greenhouseClient,
            options,
            NullLogger<GreenhouseJobService>.Instance
        );

        var jobs = await service.GetRemoteJobsAsync();

        var job = Assert.Single(jobs);

        Assert.Equal("1001", job.SourceJobId);
        Assert.Equal("Successful Company", job.CompanyName);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage>
            _responseFactory;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _responseFactory = _ => response;
        }

        public StubHttpMessageHandler(
            Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = _responseFactory(request);

            return Task.FromResult(response);
        }
    }
}
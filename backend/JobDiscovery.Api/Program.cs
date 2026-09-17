using JobDiscovery.Api.Configuration;
using Microsoft.Extensions.Options;
using JobDiscovery.Api.Clients.Ashby;
using JobDiscovery.Api.Services.Ashby;
using JobDiscovery.Api.Clients.Greenhouse;
using JobDiscovery.Api.Services.Greenhouse;
using JobDiscovery.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AshbyOptions>(
    builder.Configuration.GetSection(AshbyOptions.SectionName)
);

builder.Services.AddHttpClient<AshbyClient>(
    (serviceProvider, httpClient) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<AshbyOptions>>()
            .Value;

        httpClient.BaseAddress = new Uri(options.BaseUrl);
    }
);

builder.Services.Configure<GreenhouseOptions>(
    builder.Configuration.GetSection(GreenhouseOptions.SectionName)
);

builder.Services.AddHttpClient<GreenhouseClient>(
    (serviceProvider, httpClient) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<GreenhouseOptions>>()
            .Value;

        httpClient.BaseAddress = new Uri(options.BaseUrl);
    }
);

builder.Services.AddScoped<IJobProvider, AshbyJobService>();
builder.Services.AddScoped<IJobProvider, GreenhouseJobService>();

var app = builder.Build();

app.MapGet(
    "/api/jobs",
    async (
        AshbyJobService ashbyJobService,
        GreenhouseJobService greenhouseJobService,
        string? title,
        CancellationToken cancellationToken
    ) =>
    {
        var ashbyJobsTask = ashbyJobService.GetRemoteJobsAsync(
            cancellationToken
        );

        var greenhouseJobsTask = greenhouseJobService.GetRemoteJobsAsync(
            cancellationToken
        );

        var jobLists = await Task.WhenAll(
            ashbyJobsTask,
            greenhouseJobsTask
        );

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

        return Results.Ok(orderedJobs);
    }
);

app.Run();

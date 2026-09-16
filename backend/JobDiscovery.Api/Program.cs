using JobDiscovery.Api.Configuration;
using Microsoft.Extensions.Options;
using JobDiscovery.Api.Clients.Ashby;
using JobDiscovery.Api.Services.Ashby;
using JobDiscovery.Api.Clients.Greenhouse;
using JobDiscovery.Api.Services.Greenhouse;

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

builder.Services.AddScoped<AshbyJobService>();
builder.Services.AddScoped<GreenhouseJobService>();

var app = builder.Build();

app.MapGet(
    "/api/jobs",
    async (
        AshbyJobService ashbyJobService,
        GreenhouseJobService greenhouseJobService,
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
            .SelectMany(jobList => jobList)
            .OrderByDescending(job => job.PublishedAt)
            .ToList();

        return Results.Ok(jobs);
    }
);

app.Run();

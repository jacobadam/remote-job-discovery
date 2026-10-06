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
builder.Services.AddScoped<JobAggregationService>();

var app = builder.Build();

app.MapGet(
    "/api/jobs",
    async (
        JobAggregationService jobAggregationService,
        CancellationToken cancellationToken,
        string? title,
        int page = 1,
        int pageSize = 20
    ) =>
    {
        var jobs = await jobAggregationService.GetJobsAsync(
             title,
             page,
             pageSize,
             cancellationToken
        );

        return Results.Ok(jobs);
    }
);

app.Run();

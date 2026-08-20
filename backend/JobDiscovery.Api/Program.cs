using JobDiscovery.Api.Clients.Ashby;
using JobDiscovery.Api.Configuration;
using Microsoft.Extensions.Options;
using JobDiscovery.Api.Services.Ashby;

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

builder.Services.AddScoped<AshbyJobService>();

var app = builder.Build();

app.MapGet(
    "/api/jobs",
    async (
        AshbyJobService jobService,
        CancellationToken cancellationToken
    ) =>
    {
        var jobs = await jobService.GetRemoteJobsAsync(
            cancellationToken
        );

        return Results.Ok(jobs);
    }
);

app.Run();

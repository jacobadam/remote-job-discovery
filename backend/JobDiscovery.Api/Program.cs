using JobDiscovery.Api.Clients.Ashby;
using JobDiscovery.Api.Configuration;
using Microsoft.Extensions.Options;

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

var app = builder.Build();

app.MapGet(
    "/api/jobs",
    async ( 
        AshbyClient ashbyClient,
        CancellationToken cancellationToken
    ) =>
    {
        var response = await ashbyClient.GetJobsAsync(cancellationToken);

        var remoteJobs = response.Jobs
            .Where(job => 
                job.IsListed && 
                (
                    job.IsRemote == true || 
                    string.Equals(
                        job.WorkplaceType,
                        "Remote",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            .ToList();

        return Results.Ok(remoteJobs);
    }
);

app.Run();

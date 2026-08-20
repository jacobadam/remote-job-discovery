using JobDiscovery.Api.Clients.Ashby;
using JobDiscovery.Api.Configuration;
using Microsoft.Extensions.Options;
using JobDiscovery.Api.Models.Jobs;

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
        IOptions<AshbyOptions> ashbyOptions,
        CancellationToken cancellationToken
    ) =>
    {   
        var jobListings = new List<JobListing>();

        foreach (var comapny in ashbyOptions.Value.Companies)
        {
            var response = await ashbyClient.GetJobsAsync(
                comapny.JobBoardName,
                cancellationToken
            );

            var companyJobs = response.Jobs
                .Where(job => 
                job.IsListed &&
                string.Equals(
                    job.WorkplaceType,
                    "Remote",
                    StringComparison.OrdinalIgnoreCase
                )
            ).Select(job => new JobListing
            {
                Source = "Ashby",
                SourceJobId = job.Id,
                CompanyName = comapny.Name,
                Title = job.Title.Trim(),
                Location = job.Location.Trim(),
                WorkplaceType = job.WorkplaceType,
                PublishedAt = job.PublishedAt,
                JobUrl = job.JobUrl,
                ApplyUrl = job.ApplyUrl
            });

            jobListings.AddRange(companyJobs);
        }
        var orderedJobs = jobListings
            .OrderByDescending(job => job.PublishedAt)
            .ToList();

        return Results.Ok(orderedJobs);
    }
);

app.Run();


using JobDiscovery.Api.Clients.Ashby;
using JobDiscovery.Api.Configuration;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AshbyOptions>(
    builder.Configuration.GetSection(AshbyOptions.SectionName)
);

builder.Services.AddHttpClient<AshbyClient>(
    (ServiceProvider, HttpClient) =>
    {
        var options = ServiceProvider
            .GetRequiredService<IOptions<AshbyOptions>>()
            .Value;

        HttpClient.BaseAddress = new Uri(options.BaseUrl);
    }
);
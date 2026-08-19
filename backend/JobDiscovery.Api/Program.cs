
using JobDiscovery.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AshbyOptions>(
    builder.Configuration.GetSection(AshbyOptions.SectionName)
);
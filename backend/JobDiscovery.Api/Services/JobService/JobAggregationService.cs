namespace JobDiscovery.Api.Services;

public class JobAggregationService
{
    private readonly IEnumerable<IJobProvider> _jobProviders;

    public JobAggregationService(
      IEnumerable<IJobProvider> jobProviders
    )
    {
        _jobProviders = jobProviders;
    }
}
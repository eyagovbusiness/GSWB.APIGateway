using Common.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TGF.CA.Infrastructure.Communication.Health;
using TGF.CA.Infrastructure.Discovery;

namespace APIGateway.Infrastructure.HealthChecks.Microservices
{
    public class SwarmBot_HealthCheck : ServiceHealthCheckBase
    {
        public SwarmBot_HealthCheck(IHttpClientFactory aHttpClientFactory, IServiceDiscovery aServiceDiscovery)
            : base(aHttpClientFactory, aServiceDiscovery, aServiceName: ServicesDiscoveryNames.SwarmBot)
        {
        }

        public override async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext aContext, CancellationToken aCancellationToken = default)
            => await base.CheckHealthAsync(aContext, aCancellationToken);

    }
}

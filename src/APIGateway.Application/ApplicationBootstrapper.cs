using APIGateway.Application.Contracts.Services;
using APIGateway.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace APIGateway.Application
{
    public static class ApplicationBootstrapper
    {
        public static void RegisterApplicationServices(this IServiceCollection aServiceList)
        {
            aServiceList.AddScoped<IConsentLegalService, ConsentLegalService>();
            aServiceList.AddScoped<IGetConsentLegalService, GetConsentLegalService>();

        }
    }
}

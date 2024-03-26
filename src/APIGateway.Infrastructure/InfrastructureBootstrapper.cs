using APIGateway.Application;
using APIGateway.Infrastructure.Communication.HTTP;
using APIGateway.Infrastructure.Communication.MessageConsumer;
using APIGateway.Infrastructure.HealthChecks;
using APIGateway.Infrastructure.Middleware;
using APIGateway.Infrastructure.Services;
using Common.Application;
using Common.Application.Contracts.Services;
using Common.Infrastructure;
using Common.Infrastructure.Communication.HTTP;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using TGF.CA.Infrastructure;
using TGF.CA.Infrastructure.Communication.RabbitMQ;
using TGF.CA.Infrastructure.DB.PostgreSQL;

namespace APIGateway.Infrastructure
{
    /// <summary>
    /// Provides methods for configuring and using the application specific infrastructure layer components.
    /// </summary>
    public static class InfrastructureBootstrapper
    {
        /// <summary>
        /// Configures the necessary infrastructure services for the application.
        /// </summary>
        /// <param name="aWebApplicationBuilder">The web application builder.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task ConfigureInfrastructureAsync(this WebApplicationBuilder aWebApplicationBuilder)
        {
            aWebApplicationBuilder.Services.AddReverseProxy()
            .LoadFromConfig(aWebApplicationBuilder.Configuration.GetSection("ReverseProxy"));

            await aWebApplicationBuilder.ConfigureCommonInfrastructureAsync();

            await aWebApplicationBuilder.Services.AddPostgreSQL<APIGatewayAuthDbContext>("ApiGatewayAuthDb");
            aWebApplicationBuilder.Services.AddScoped<ITokenPairAuthRecordRepository, TokenPairAuthRecordRepository>();
            aWebApplicationBuilder.Services.AddScoped<ITokenService, TokenService>();
            aWebApplicationBuilder.Services.AddSingleton<ITokenRevocationService, TokenRevocationService>();

            aWebApplicationBuilder.ConfigureCommunication();

            aWebApplicationBuilder.Services.AddHostedService<TokenCleanupService>();
            aWebApplicationBuilder.AddHealthChceckServices();

            aWebApplicationBuilder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("ip-60request/minute-limit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 60,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

        }

        /// <summary>
        /// Configure the communication-related services such as message broker related services or direct communication related services
        /// </summary>
        private static void ConfigureCommunication(this WebApplicationBuilder aWebApplicationBuilder)
        {
            aWebApplicationBuilder.Services.AddScoped<ISwarmBotCommunicationService, SwarmBotCommunicationService>();
            aWebApplicationBuilder.Services.AddScoped<IMembersCommunicationService, MembersCommunicationService>();

            aWebApplicationBuilder.Services.AddMessageHandlersInAssembly<RoleTokenRevocationHandler>();
            aWebApplicationBuilder.Services.AddServiceBusIntegrationConsumer();
        }

        /// <summary>
        /// Applies the infrastructure configurations to the web application.
        /// </summary>
        /// <param name="aWebApplication">The Web application instance.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task UseInfrastructure(this WebApplication aWebApplication)
        {
            aWebApplication.UseMiddleware<BlockPrivateProxyingMiddleware>();
            aWebApplication.UseCommonInfrastructure();
            await aWebApplication.UseMigrations<APIGatewayAuthDbContext>();
            aWebApplication.UseMiddleware<TokenFilterMiddleware>();
            aWebApplication.MapReverseProxy();

        }

    }
}

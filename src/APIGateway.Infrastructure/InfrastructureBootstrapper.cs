using APIGateway.Application;
using APIGateway.Infrastructure.Communication.MessageConsumer;
using APIGateway.Infrastructure.HealthChecks;
using APIGateway.Infrastructure.Middleware;
using APIGateway.Infrastructure.Services;
using Common.Application.Contracts.Services;
using Common.Infrastructure;
using Common.Infrastructure.Communication.HTTP;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.RateLimiting;
using TGF.CA.Application;
using TGF.CA.Infrastructure;
using TGF.CA.Infrastructure.Comm.RabbitMQ;
using TGF.CA.Infrastructure.DB.PostgreSQL;
using TGF.CA.Infrastructure.DB.Repository;

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

            aWebApplicationBuilder.Services.AddScoped<IEncryptionService, EncryptionService>();

            await aWebApplicationBuilder.Services.AddPostgreSQL<AuthDbContext>("AuthDb");
            await aWebApplicationBuilder.Services.AddPostgreSQL<LegalDbContext>("LegalDb");
            aWebApplicationBuilder.Services.AddRepositories(Assembly.GetExecutingAssembly());
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
            aWebApplicationBuilder.Services.AddScoped<IAllowMeCommunicationService, AllowMeCommunicationService>();

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
            // Forwarded headers middleware should come first
            aWebApplication.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // HTTPS redirection middleware
            aWebApplication.UseHttpsRedirection();

            // Custom middleware for blocking private proxying
            aWebApplication.UseMiddleware<BlockPrivateProxyingMiddleware>();

            // Common infrastructure middleware
            await aWebApplication.UseCommonInfrastructure();

            // Apply migrations (ensure the database is up to date)
            await aWebApplication.UseMigrations<AuthDbContext>();
            await aWebApplication.UseMigrations<LegalDbContext>();

            // Token filter middleware
            aWebApplication.UseMiddleware<TokenFilterMiddleware>();

            // Map reverse proxy
            aWebApplication.MapReverseProxy();
        }


    }
}

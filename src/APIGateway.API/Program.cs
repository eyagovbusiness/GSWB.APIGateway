using APIGateway.API;
using APIGateway.Application;
using APIGateway.Infrastructure;
using Common.Presentation;
using Microsoft.AspNetCore.HttpOverrides;


WebApplicationBuilder lAPIGatewayApplicationBuilder = WebApplication.CreateBuilder(args);

await lAPIGatewayApplicationBuilder.ConfigureInfrastructureAsync();
lAPIGatewayApplicationBuilder.Services.RegisterApplicationServices();
lAPIGatewayApplicationBuilder.ConfigureCommonPresentation();
lAPIGatewayApplicationBuilder.ConfigurePresentation();

var lAPIGatewayWebApplication = lAPIGatewayApplicationBuilder.Build();
lAPIGatewayWebApplication.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configurar el middleware para redirigir HTTP a HTTPS
lAPIGatewayWebApplication.UseHttpsRedirection();
await lAPIGatewayWebApplication.UseInfrastructure();
lAPIGatewayWebApplication.UsePresentation();

await lAPIGatewayWebApplication.RunAsync();


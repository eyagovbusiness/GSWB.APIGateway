using APIGateway.API;
using APIGateway.Application;
using APIGateway.Infrastructure;
using Common.Presentation;


WebApplicationBuilder lAPIGatewayApplicationBuilder = WebApplication.CreateBuilder(args);

await lAPIGatewayApplicationBuilder.ConfigureInfrastructureAsync();
lAPIGatewayApplicationBuilder.Services.RegisterApplicationServices();
lAPIGatewayApplicationBuilder.ConfigureCommonPresentation();
lAPIGatewayApplicationBuilder.ConfigurePresentation();

var lAPIGatewayWebApplication = lAPIGatewayApplicationBuilder.Build();

await lAPIGatewayWebApplication.UseInfrastructure();
lAPIGatewayWebApplication.UsePresentation();

await lAPIGatewayWebApplication.RunAsync();


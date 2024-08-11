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


// TEMPORARY: Configure Kestrel to use HTTPS with the provided PFX certificate
lAPIGatewayApplicationBuilder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000); // HTTP port
    serverOptions.ListenAnyIP(5001, listenOptions =>
    {
        var certPath = Path.Combine(lAPIGatewayApplicationBuilder.Environment.ContentRootPath + "/certs", "combined.pfx");
        var certPassword = Environment.GetEnvironmentVariable("PFX_KEY");

        listenOptions.UseHttps(certPath, certPassword);
    }); // HTTPS port
});

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




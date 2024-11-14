using APIGateway.API;
using APIGateway.Application;
using APIGateway.Infrastructure;
using Common.Domain;
using Common.Presentation;


WebApplicationBuilder lAPIGatewayApplicationBuilder = WebApplication.CreateBuilder(args);

lAPIGatewayApplicationBuilder.ConfigureCommonDomain();
await lAPIGatewayApplicationBuilder.ConfigureInfrastructureAsync();
lAPIGatewayApplicationBuilder.Services.RegisterApplicationServices();
lAPIGatewayApplicationBuilder.ConfigureCommonPresentation();
lAPIGatewayApplicationBuilder.ConfigurePresentation();

if (!lAPIGatewayApplicationBuilder.Environment.IsDevelopment())
// TEMPORARY: Configure Kestrel to use HTTPS with the provided PFX certificate
lAPIGatewayApplicationBuilder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000); // HTTP port
    serverOptions.ListenAnyIP(5001, listenOptions =>
    {   
        var certRelativePath = Environment.GetEnvironmentVariable("PFX_PATH");
        if (string.IsNullOrEmpty(certRelativePath))
            throw new Exception("PFX_PATH environment variable is not set");
        var certPath = Path.Combine(lAPIGatewayApplicationBuilder.Environment.ContentRootPath, certRelativePath);
        var certPassword = Environment.GetEnvironmentVariable("PFX_KEY");
        if (string.IsNullOrEmpty(certPassword))
            throw new Exception("PFX_KEY environment variable is not set");

        listenOptions.UseHttps(certPath, certPassword);
    }); // HTTPS port
});

var lAPIGatewayWebApplication = lAPIGatewayApplicationBuilder.Build();

await lAPIGatewayWebApplication.UseInfrastructure();
lAPIGatewayWebApplication.UsePresentation();

await lAPIGatewayWebApplication.RunAsync();




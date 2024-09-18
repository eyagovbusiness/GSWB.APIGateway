using APIGateway.API.Endpoints;
using FluentValidation;
using HealthChecks.UI.Client;
using APIGateway.API.Validation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;
using TGF.CA.Application;
using TGF.CA.Presentation;
using TGF.CA.Presentation.Middleware;
using TGF.CA.Presentation.MinimalAPI;
using Common.Presentation;

namespace APIGateway.API
{
    /// <summary>
    /// Provides methods for configuring and using the application specific presentation layer components.
    /// </summary>
    public static class PresentationBootstrapper
    {
        /// <summary>
        /// Make the configurations at the presentation layer for this web application.
        /// </summary>
        public static void ConfigurePresentation(this WebApplicationBuilder aWebApplicationBuilder)
        {
            var lXmlDocFileName = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
            var lXmlDocFilePath = Path.Combine(AppContext.BaseDirectory, lXmlDocFileName);

            aWebApplicationBuilder.ConfigureDefaultPresentation(
                new List<string> { lXmlDocFilePath },
                aScanMarkerList: typeof(IdentityEndpoints),
                aIsHttps:true
            );

            aWebApplicationBuilder.ConfigureFrontendCORS(aWebApplicationBuilder.Configuration);
            aWebApplicationBuilder.Services.AddValidatorsFromAssemblyContaining<RefreshTokenValidator>();
        }

        /// <summary>
        /// Applies the presentation configurations to the web application and setup the middleware pipeline
        /// </summary>
        public static void UsePresentation(this WebApplication aWebApplication)
        {
            aWebApplication.UseCommonPresentation();

            //aWebApplication.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto });
            aWebApplication.UseCustomErrorHandlingMiddleware();
            aWebApplication.UseFrontendCORS();//CORS should be placed before routing.
            aWebApplication.UseRouting();//UseRouting() must be called before UseAuthentication() and UseAuthorization() which is UseIdentity().
            aWebApplication.UseIdentity();

            aWebApplication.MapHealthChecksUI(options => options.UIPath = TGFEndpointRoutes.healthUi);
            aWebApplication.UseEndpointDefinitions();
        }

    }
}

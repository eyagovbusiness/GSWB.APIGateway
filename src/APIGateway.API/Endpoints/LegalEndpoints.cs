using APIGateway.Application.Contracts.Services;
using Common.Application.DTOs.Legal;
using Common.Infrastructure.Communication.ApiRoutes;
using TGF.CA.Presentation;
using TGF.CA.Presentation.Middleware;
using TGF.CA.Presentation.MinimalAPI;

namespace APIGateway.API.Endpoints
{

    /// <inheritdoc/>
    public class LegalEndpoints : IEndpointDefinition
    {

        #region IEndpointDefinition

        /// <inheritdoc/>
        public void DefineEndpoints(WebApplication aWebApplication)
        {
            aWebApplication.MapPost(APIGatewayApiRoutes.Legal_consent.Route, Post_ConsentLog).SetResponseMetadata(200);
        }

        /// <inheritdoc/>
        public void DefineRequiredServices(IServiceCollection aRequiredServicesCollection)
        {

        }

        #endregion

        #region EndpointMethods

        /// <summary>
        /// Creates a Consent log in the legal database with the user consent about our privacy policiy and other future legal consents we require from the user.
        /// </summary>
        private async Task<IResult> Post_ConsentLog(ConsentLogDTO aConsentLogDto, HttpContext aHttpContext, IConsentLegalService aConsentLegalService, CancellationToken aCancellationToken = default)
        {

            // Capture the client's IP address from the HttpContext
            var lRemoteIpAddress = aHttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(lRemoteIpAddress))
                return Results.BadRequest("IP address could not be determined.");

            return await aConsentLegalService.ConsentLegal(lRemoteIpAddress, aConsentLogDto, aCancellationToken)
                .ToIResult();

        }

        #endregion

    }

}

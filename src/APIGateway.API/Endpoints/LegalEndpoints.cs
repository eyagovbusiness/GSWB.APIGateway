using APIGateway.Application.Contracts.Services;
using Common.Application.Communication.Routing;
using Common.Application.DTOs.Legal;
using TGF.CA.Presentation;
using TGF.CA.Presentation.MinimalAPI;
using static TGF.CA.Presentation.ApiRoutes;

namespace APIGateway.API.Endpoints
{

    /// <inheritdoc/>
    public class LegalEndpoints : IEndpointsDefinition
    {

        #region IEndpointDefinition

        /// <inheritdoc/>
        public void DefineEndpoints(WebApplication aWebApplication)
        {
            aWebApplication.MapGet(APIGatewayApiRoutes.Legal_consent.Route + Identifiers.Id, Get_ConsentLog).SetResponseMetadata<ConsentLogDTO>(200);
            aWebApplication.MapPost(APIGatewayApiRoutes.Legal_consent.Route, Post_ConsentLog).SetResponseMetadata<Guid>(200);

        }

        /// <inheritdoc/>
        public void DefineRequiredServices(IServiceCollection aRequiredServicesCollection)
        {

        }

        #endregion

        #region EndpointMethods
        /// <summary>
        /// Get an updated Consent from the legal database with the user consent about our privacy policiy and other future legal consents we require from the user.
        /// </summary>
        private async Task<IResult> Get_ConsentLog(Guid id, HttpContext aHttpContext, IGetConsentLegalService aGetConsentLegalService, CancellationToken aCancellationToken = default)
        {
            // Capture the client's IP address from the HttpContext
            var lRemoteIpAddress = aHttpContext?.Connection?.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(lRemoteIpAddress))
                return Results.BadRequest("IP address could not be determined.");

            return await aGetConsentLegalService.GetConsentLegal(lRemoteIpAddress, id, aCancellationToken)
                .ToIResult();

        }

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

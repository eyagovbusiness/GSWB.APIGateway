using APIGateway.Application;
using APIGateway.Application.DTOs;
using APIGateway.Infrastructure.Helpers.Token;
using Common.Application.Contracts.ApiRoutes;
using Common.Application.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TGF.CA.Application;
using TGF.CA.Infrastructure.Security.Identity.Authentication;
using TGF.CA.Presentation;
using TGF.CA.Presentation.Middleware;
using TGF.CA.Presentation.MinimalAPI;

namespace APIGateway.API.Endpoints
{

    /// <inheritdoc/>
    public class IdentityEndpoints : IEndpointDefinition
    {

        #region IEndpointDefinition

        /// <inheritdoc/>
        public void DefineEndpoints(WebApplication aWebApplication)
        {
            aWebApplication.MapGet(APIGatewayApiRoutes.auth_signIn, Get_SignIn).RequireDiscord().SetResponseMetadata(301);
            aWebApplication.MapPut(APIGatewayApiRoutes.auth_signUp, Put_SignUp).RequireDiscord().SetResponseMetadata<MemberDetailDTO>(200, 400);
            aWebApplication.MapGet(APIGatewayApiRoutes.auth_signOut, Get_SignOut).RequireDiscord().SetResponseMetadata(200);
            aWebApplication.MapGet(APIGatewayApiRoutes.auth_token, Get_TokenPair).RequireDiscord().SetResponseMetadata<TokenPairDTO>(200, 404);
            aWebApplication.MapPut(APIGatewayApiRoutes.auth_refreshToken, Put_AccessTokenRefresh).SetResponseMetadata<string>(200, 400, 404);
            aWebApplication.MapGet(TGFEndpointRoutes.auth_OAuthFailed, Get_OAuthFiled).SetResponseMetadata(301);
        }

        /// <inheritdoc/>
        public void DefineRequiredServices(IServiceCollection aRequiredServicesCollection)
        {

        }

        #endregion

        #region EndpointMethods

        /// <summary>
        /// Triggers Discord OAuth in the backend resulting on success in a redirect response where an HTTP only server side cookie "PreAuthCookie" will be attached including information about the authenticated discord user.The redirection is protected by CORS policy.
        /// </summary>
        private IResult Get_SignIn(HttpContext aHttpContext, IConfiguration aConfiguration)
        {
            aHttpContext.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
            aHttpContext.Response.Headers.Append("Pragma", "no-cache");
            aHttpContext.Response.Headers.Append("Expires", "0");

            return Results.Redirect(aConfiguration.GetValue<string>("FrontendURL") + aConfiguration.GetValue<string>("AuthCallbackURI"), permanent: true);
        }

        /// <summary>
        /// Creates a member account in database using the "PreAuthCookie" retrieved after /signIn. The response contains the details about the new member created.
        /// </summary>
        private async Task<IResult> Put_SignUp([FromBody] SignUpDataDTO? aSignUpData, ClaimsPrincipal aClaims, IMembersCommunicationService aMembersCommunicationService, CancellationToken aCancellationToken = default)
            => await aMembersCommunicationService.SignUpNewMember(aSignUpData, TokenGenerationHelpers.GetDiscordCookieUserInfo(aClaims), aCancellationToken)
            .ToIResult();

        /// <summary>
        /// Get a new pair of access token and refresh token using the "PreAuthCookie" retrieved after /signIn.
        /// </summary>
        private async Task<IResult> Get_TokenPair(ITokenService aTokenService, ClaimsPrincipal aClaims, CancellationToken aCancellationToken = default)
            => await aTokenService.GetNewTokenPairAsync(aClaims, aCancellationToken)
            .ToIResult();

        /// <summary>
        /// Refresh the expired access token. Requires both access token and refresh tokens sent in the request's body.
        /// </summary>
        private async Task<IResult> Put_AccessTokenRefresh([FromBody] TokenPairDTO aTokenPair, ITokenService aTokenService, CancellationToken aCancellationToken = default)
            => await aTokenService.GetRefreshedAccessTokenAsync(aTokenPair, aCancellationToken)
            .ToIResult();

        /// <summary>
        /// SignOut the current user revoking the HTTP only cookie generated after Discord OAuth.
        /// </summary>
        private IResult Get_SignOut(HttpContext aHttpContext)
            => Results.SignOut(authenticationSchemes: new string[] { CookieAuthenticationDefaults.AuthenticationScheme });

        /// <summary>
        /// Callback endpoint when OAuth with Discord fails.Redirects to the frontend.The redirection is protected by CORS policy.
        /// </summary>
        private IResult Get_OAuthFiled(HttpContext aHttpContext, IConfiguration aConfiguration)
        {
            aHttpContext.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
            aHttpContext.Response.Headers.Append("Pragma", "no-cache");
            aHttpContext.Response.Headers.Append("Expires", "0");

            return Results.Redirect(aConfiguration.GetValue<string>("FrontendURL") + aConfiguration.GetValue<string>("AuthCallbackFailedURI"), permanent: true);
        }

        #endregion

    }

}

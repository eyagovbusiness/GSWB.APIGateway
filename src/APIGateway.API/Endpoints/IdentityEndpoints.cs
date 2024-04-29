using APIGateway.API.Validation;
using APIGateway.Application;
using APIGateway.Application.DTOs;
using APIGateway.Infrastructure.Helpers.Token;
using Common.Application.Contracts.Services;
using Common.Application.DTOs.Auth;
using Common.Application.DTOs.Members;
using Common.Infrastructure.Communication.ApiRoutes;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TGF.CA.Application;
using TGF.CA.Infrastructure.Security.Identity.Authentication;
using TGF.CA.Presentation;
using TGF.CA.Presentation.Middleware;
using TGF.CA.Presentation.MinimalAPI;
using TGF.Common.ROP.HttpResult;
using TGF.Common.ROP.Result;

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
            aWebApplication.MapPut(APIGatewayApiRoutes.auth_signOut, Put_SignOut).RequireDiscord().RequireJWTBearer().SetResponseMetadata(200, 400, 404);
            aWebApplication.MapGet(APIGatewayApiRoutes.auth_token, Get_TokenPair).RequireDiscord().SetResponseMetadata<TokenPairDTO>(200, 404);
            aWebApplication.MapPut(APIGatewayApiRoutes.auth_token_refresh, Put_AccessTokenRefresh).SetResponseMetadata<string>(200, 400, 404);
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
        private async Task<IResult> Put_SignOut([FromBody] TokenPairDTO aTokenPair, HttpContext aHttpContext, RefreshTokenValidator aRefreshTokenValidator, ITokenService aTokenService, ITokenRevocationService aTokenRevocationService, CancellationToken aCancellationToken = default)
            => await Result.CancellationTokenResult(aCancellationToken)
            .Validate(aTokenPair, aRefreshTokenValidator)
            .Map(_ => Results.SignOut(authenticationSchemes: [CookieAuthenticationDefaults.AuthenticationScheme]).ExecuteAsync(aHttpContext))
            .Verify(signOutResult => signOutResult.IsCompletedSuccessfully, Infrastructure.InfrastructureErrors.Identity.CookieSignOutFailure)
            .Bind(_ => aTokenService.OnSignOutTokenCleanupAsync(aTokenPair.RefreshToken, aCancellationToken))
            .Tap(_ => aTokenRevocationService.BlacklistAccessTokenList([aTokenPair.AccessToken]))
            .ToIResult();

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

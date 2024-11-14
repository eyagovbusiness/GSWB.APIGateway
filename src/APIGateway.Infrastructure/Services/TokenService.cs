using APIGateway.Application;
using APIGateway.Application.DTOs;
using APIGateway.Application.Mapping;
using APIGateway.Domain.Entities;
using Common.Application.Contracts.Services;
using Common.Application.DTOs.Auth;
using Common.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TGF.CA.Application;
using TGF.Common.ROP;
using TGF.Common.ROP.Errors;
using TGF.Common.ROP.HttpResult;
using TGF.Common.ROP.Result;
using TGF.Common.ROP.HttpResult.RailwaySwitches;
using static APIGateway.Infrastructure.Helpers.Token.TokenGenerationHelpers;
using static APIGateway.Infrastructure.Helpers.Token.TokenValidationHelpers;
using Common.Domain.ValueObjects;

namespace APIGateway.Infrastructure.Services
{
    /// <summary>
    /// Service that provides AccessToken-RefreshToken generation, refreshing AccessTokens, and revocation of the pair.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </remarks>
    /// <param name="aTokenPairAuthRecordRepository">The <see cref="TokenPairAuthRecord"/> authentication record repository.</param>
    /// <param name="aSecretsManager">The secrets manager.</param>
    internal class TokenService(
        ITokenPairAuthRecordRepository aTokenPairAuthRecordRepository,
        ISecretsManager aSecretsManager,
        IMembersCommunicationService aMembersCommunicationService,
        IConfiguration aConfiguration) : ITokenService
    {
        #region Private variables
        private readonly ITokenPairAuthRecordRepository _tokenPairAuthRecordRepository = aTokenPairAuthRecordRepository 
            ?? throw new ArgumentNullException(nameof(aTokenPairAuthRecordRepository));

        private readonly ISecretsManager _secretsManager = aSecretsManager 
            ?? throw new ArgumentNullException(nameof(aSecretsManager));

        private readonly IMembersCommunicationService _membersCommunicationService = aMembersCommunicationService;
        private readonly TimeSpan _accessTokenLifetime = DefaultTokenLifetimes.AccessToken;
        private readonly TimeSpan _refreshTokenLifetime = DefaultTokenLifetimes.RefreshToken;

        private readonly string _issuer = aConfiguration.GetValue<string>("FrontendURL")
                ?? throw new Exception("Error while configuring the default presentation, FrontendURL was not found in appsettings. Please add this configuration.");

        private readonly string _securityAlg = SecurityAlgorithms.HmacSha256;
        #endregion

        #region ITokenService implementation

        public async Task<IHttpResult<TokenPairDTO>> GetNewTokenPairAsync(string guildId, ClaimsPrincipal aClaimsPrincipal, CancellationToken aCancellationToken = default)
        {
            DiscordCookieUserInfo? lDiscordCookieUserInfo = default;
            return await Result.CancellationTokenResultAsync(aCancellationToken)
                .Tap(_ => lDiscordCookieUserInfo = GetDiscordCookieUserInfo(aClaimsPrincipal))
                .Bind(_ => _membersCommunicationService.GetExistingMember(new MemberKey(guildId,lDiscordCookieUserInfo!.UserNameIdentifier), aCancellationToken))
                .Bind(memberDTO => GetNewClaims(lDiscordCookieUserInfo!, memberDTO, _issuer, _issuer))
                .Bind(claims => GenerateNewTokenPairAsync(claims, aCancellationToken));

        }

        public async Task<IHttpResult<string>> GetRefreshedAccessTokenAsync(TokenPairDTO aTokenPair, CancellationToken aCancellationToken = default)
        {
            var lValidationTokenResult = await Result.CancellationTokenResult(aCancellationToken)
                .Bind(_ => GetValidationTokenResult(aTokenPair.AccessToken, aCancellationToken));

            var lTokenPairAuthDBRecordResult = await lValidationTokenResult
                .Bind(_ => _tokenPairAuthRecordRepository.GetByRefreshTokenAsync(aTokenPair.RefreshToken, aCancellationToken));

            if (!lTokenPairAuthDBRecordResult.IsSuccess)
                return Result.Failure<string>(InfrastructureErrors.AuthTokenRefresh.BadTokenRefreshRequest.InvalidRefreshToken);

            return await CheckValidationTokenResultCanBeRefreshed(lValidationTokenResult.Value, aTokenPair, lTokenPairAuthDBRecordResult.Value, _securityAlg)
                .Bind(validationTokenResult => RefreshAndSaveTokenPair(validationTokenResult.ClaimsPrincipal, lTokenPairAuthDBRecordResult.Value))
                .Verify(token => token != null && token.Length >= 1, InfrastructureErrors.AuthTokenRefresh.ServerError);
        }

        public async Task<IHttpResult<ImmutableArray<string>>> OutdateTokenPairForMemberListAsync(IEnumerable<MemberKey> memberIdList, CancellationToken aCancellationToken = default)
        => await _tokenPairAuthRecordRepository.RevokeByDiscordUserIdListAsync(memberIdList, aCancellationToken);

        public async Task<IHttpResult<ImmutableArray<string>>> OutdateTokenPairForRoleListAsync(IEnumerable<RoleKey> aRoleIdList, CancellationToken aCancellationToken = default)
        => await _tokenPairAuthRecordRepository.RevokeByDiscordRoleIdListAsync(aRoleIdList, aCancellationToken);

        public async Task<IHttpResult<Unit>> OnSignOutTokenCleanupAsync(string aRefreshToken, CancellationToken aCancellationToken = default)
        => await _tokenPairAuthRecordRepository.DeleteByRefreshTokenAsync(aRefreshToken, aCancellationToken);

        #endregion

        #region Private helpers

        /// <summary>
        /// Generates a new JWT Bearer token with the specified claims.
        /// </summary>
        /// <param name="aClaimList">List of claims for the new token.</param>
        /// <returns><see cref="string"/> representing the generated JWT Bearer token.</returns>
        private async Task<IHttpResult<string>> GenerateAccessToken(IEnumerable<Claim> aClaimList, CancellationToken aCancellationToken = default)
        {
            var lTokenHandler = new JwtSecurityTokenHandler();
            return await Result.CancellationTokenResult(aCancellationToken)
                .Map(async _ => Encoding.UTF8.GetBytes(await _secretsManager.GetTokenSecret(DefaultTokenNames.AccessToken)))
                .Map(key => new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(aClaimList),
                    Expires = DateTimeOffset.Now.UtcDateTime.Add(_accessTokenLifetime),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), _securityAlg)
                })
                .Map(tokenDescriptor => lTokenHandler.CreateToken(tokenDescriptor))
                .Map(securityToken => lTokenHandler.WriteToken(securityToken));
        }

        /// <summary>
        /// Asynchronously generates a new pair of access and refresh tokens based on the provided claims.
        /// </summary>
        /// <param name="aClaims">The claims to be encoded in the access token.</param>
        /// <param name="aCancellationToken">An optional token to monitor for cancellation requests.</param>
        /// <returns>
        /// A result containing the generated pair of tokens if successful; otherwise, a result indicating the error.
        /// </returns>
        private async Task<IHttpResult<TokenPairDTO>> GenerateNewTokenPairAsync(IEnumerable<Claim> aClaims, CancellationToken aCancellationToken = default)
        {
            var lGetAccessTokenResult = await Result.CancellationTokenResult(aCancellationToken)
                .Bind(_ => GenerateAccessToken(aClaims, aCancellationToken));

            ulong lDiscordGuildId = Convert.ToUInt64(aClaims.First(claim => claim.Type == GuildSwarmClaims.GuildId).Value);//new
            ulong lDiscordUserId = Convert.ToUInt64(aClaims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
            ulong lDiscordRoleId = Convert.ToUInt64(aClaims.First(claim => claim.Type == ClaimTypes.Role).Value);
            string lRefreshToken = default!;
            return await lGetAccessTokenResult
                .Tap(accessToken => lRefreshToken = GenerateRefreshToken())
                .Bind(accessToken => SaveNewTokenPairAuthRecordAsync(accessToken, lRefreshToken!, new MemberKey(lDiscordGuildId, lDiscordUserId), new RoleKey(lDiscordGuildId, lDiscordRoleId), aCancellationToken))
                .Map(newTokenPairAuthRecord => newTokenPairAuthRecord.ToDto());
        }

        /// <summary>
        /// Takes a string representing a JWT Bearer token and verifies if the token was issued by this service, in such case generates a new instance of otherwise an exception will be thrown.
        /// </summary>
        /// <param name="aAccessToken"><see cref="string"/> representing the access token.</param>
        /// <returns>A new instance of <see cref="ValidationTokenResult"/> with an instance of the token as <see cref="JwtSecurityToken"/> and the <see cref="ClaimsPrincipal"/> from the provided access token string.</returns>
        private async Task<IHttpResult<ValidationTokenResult>> GetValidationTokenResult(string aAccessToken, CancellationToken aCancellationToken)
        => (await Result.CancellationTokenResult(aCancellationToken)
        .Map(async _ =>
        {
            var JwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var lKey = Encoding.UTF8.GetBytes(await _secretsManager.GetTokenSecret(DefaultTokenNames.AccessToken));
            var lTokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(lKey),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _issuer,
                ValidateLifetime = false//do not validate lifetime
            };
            return new ValidationTokenResult(
                    JwtSecurityTokenHandler.ValidateToken(aAccessToken, lTokenValidationParameters, out SecurityToken lValidSecurityToken)
                    , (lValidSecurityToken as JwtSecurityToken)!);
        }))
        .Verify(validateTokenResultStruct => validateTokenResultStruct.SecurityToken != null, InfrastructureErrors.AuthTokenRefresh.ServerError);

        #region DataAccess

        /// <summary>
        /// Stores in the auth database the pair of Access and Refresh tokens, making persistent the relationship between both for future refresh requests.
        /// </summary>
        /// <returns>The new record that was created or Error.</returns>
        private async Task<IHttpResult<TokenPairAuthRecord>> SaveNewTokenPairAuthRecordAsync(string aAccessToken, string aRefreshToken, MemberKey memberId, RoleKey roleId, CancellationToken aCancellationToken = default)
        => await Result.CancellationTokenResult(aCancellationToken)
        .Map(_ => new TokenPairAuthRecord(memberId, roleId)
        {
            AccessToken = aAccessToken,
            RefreshToken = aRefreshToken,
            IsOutdated = false,
            ExpiryDate = DateTimeOffset.Now.Add(_refreshTokenLifetime)
        })
        .Bind(newTokenPairAuthRecord => _tokenPairAuthRecordRepository.AddAsync(newTokenPairAuthRecord, aCancellationToken));


        /// <summary>
        /// Generates a new JWT Bearer token from updating he given claims if outdated and extending the expirty date.
        /// </summary>
        /// <param name="aClaimsPrincipal">Claims from the original token that has to be refreshed.</param>
        /// <param name="aTokenPairAuthRecord">The record in database associated with the AccessToken being refreshed.</param>
        /// <returns><see cref="string"/> representing the new JWT Bearer token.</returns>
        private async Task<IHttpResult<string>> RefreshAndSaveTokenPair(ClaimsPrincipal aClaimsPrincipal, TokenPairAuthRecord aTokenPairAuthRecord, CancellationToken aCancellationToken = default)
        => await Result.CancellationTokenResult(aCancellationToken)
            .Bind(_ => GetRefreshedAccessToken(aClaimsPrincipal, aTokenPairAuthRecord, aCancellationToken))
            .Tap(newAccessToken =>
            {
                aTokenPairAuthRecord.AccessToken = newAccessToken;
                if (aTokenPairAuthRecord.IsOutdated)
                    aTokenPairAuthRecord.IsOutdated = false;
            })
            .Tap(_ => _tokenPairAuthRecordRepository.UpdateAsync(aTokenPairAuthRecord))
            .Verify(saveBoolResult => !aCancellationToken.IsCancellationRequested, CommonErrors.CancellationToken.Cancelled)
            .Verify(newAccessToken => newAccessToken != null, CommonErrors.CancellationToken.Cancelled);

        /// <summary>
        /// Get the refreshed access token with extended expiry date(<see cref="SecurityTokenDescriptor.Expires"/>) and updated permissions if the record was outdated(<see cref="TokenPairAuthRecord.IsOutdated"/>).
        /// </summary>
        /// <param name="aClaimsPrincipal">Claims from the expired access token sent to be refreshed.</param>
        /// <param name="aTokenPairAuthRecord">Related token pair from auth DB.</param>
        /// <returns>The new access token after refresh.</returns>
        private async Task<IHttpResult<string>> GetRefreshedAccessToken(ClaimsPrincipal aClaimsPrincipal, TokenPairAuthRecord aTokenPairAuthRecord, CancellationToken aCancellationToken = default)
        => await GetUpdatedClaims(_membersCommunicationService, aClaimsPrincipal, aTokenPairAuthRecord, aCancellationToken)
            .Bind(updatedClaimList => GenerateAccessToken(updatedClaimList, aCancellationToken));


        #endregion 

        #endregion

    }

}

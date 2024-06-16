using APIGateway.Application.DTOs;
using APIGateway.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TGF.Common.ROP.HttpResult;
using TGF.Common.ROP.Result;

namespace APIGateway.Infrastructure.Helpers.Token
{
    internal static class TokenValidationHelpers
    {

        /// <summary>
        /// Check if the <see cref="ValidationTokenResult"/> is valid according to DB records and then verifies the security algorythim is also valid.
        /// The method ensures the security algorithm from the reconstructed token header matches the sec alg used by this service to expedite tokens and then verifies the token was registered in the AuthDb(<see cref="TokenPairAuthRecord"/>).
        /// </summary>
        /// <param name="aValidationTokenResult">The result after validating the provided access token to refresh with an instance of the token as <see cref="JwtSecurityToken"/> and the <see cref="ClaimsPrincipal"/> from the provided access token string.</param>
        /// <param name="aTokenPair">Pair of tokens with the AccessToken requested to be refreshed and the RefreshToken associated to tis AccessToken.</param>
        /// <param name="aTokenPairAuthDBRecord">The database record(if it was found) associated with the token requested to refresh.</param>
        /// <param name="aSecurityAlg">The security algorithm used by this service to expedite tokens.</param>
        internal static IHttpResult<ValidationTokenResult> CheckValidationTokenResultCanBeRefreshed(ValidationTokenResult aValidationTokenResult, TokenPairDTO aTokenPair, TokenPairAuthRecord? aTokenPairAuthDBRecord, string aSecurityAlg)
        => (aTokenPairAuthDBRecord != null
            ? Result.SuccessHttp(aValidationTokenResult) : Result.Failure<ValidationTokenResult>(InfrastructureErrors.AuthDatabase.AccessTokenNotFound))
            .Verify(_ => aValidationTokenResult.SecurityToken?.Header.Alg.Equals(aSecurityAlg, StringComparison.InvariantCulture) == true
                            && aTokenPairAuthDBRecord!.AccessToken == aTokenPair.AccessToken
                        , InfrastructureErrors.AuthDatabase.AccessTokenNotFound)
            .Bind(_ => CanAccessTokenBeRefreshed(aValidationTokenResult, aTokenPairAuthDBRecord!));

        /// <summary>
        /// Returns success if the validated access token can be refreshed (this is if its related <see cref="TokenPairAuthRecord"/> is outdfated in DB or <see cref="ValidationTokenResult.SecurityToken"/> has indeed expired).
        /// </summary>
        /// <param name="aValidationTokenResult">The result after validating the provided access token to refresh with an instance of the token as <see cref="JwtSecurityToken"/> and the <see cref="ClaimsPrincipal"/> from the provided access token string.</param>
        /// <param name="aTokenPairAuthDBRecord">The database record(if it was found) associated with the token requested to refresh.</param>
        /// <returns>True if it has expired already, false in case it has not expired yet.</returns>
        private static IHttpResult<ValidationTokenResult> CanAccessTokenBeRefreshed(ValidationTokenResult aValidationTokenResult, TokenPairAuthRecord aTokenPairAuthDBRecord)
        {
            if (aTokenPairAuthDBRecord.IsOutdated)
                return Result.SuccessHttp(aValidationTokenResult);

            return IsAccessTokenExpired(aValidationTokenResult)
                 ? Result.SuccessHttp(aValidationTokenResult) : Result.Failure<ValidationTokenResult>(InfrastructureErrors.AuthTokenRefresh.BadTokenRefreshRequest.NotExpired);
        }

        /// <summary>
        /// Verifies that the reconstructed access token has expired already by its Claims.
        /// </summary>
        /// <returns>True if it has expired already, false in case it has not expired yet.</returns>
        private static bool IsAccessTokenExpired(ValidationTokenResult aValidationTokenResult)
        {
            var lExpiration = aValidationTokenResult.ClaimsPrincipal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Exp).Value;
            var lExpirationDate = UnixTimeStampToDateTime(double.Parse(lExpiration));
            return lExpirationDate <= DateTimeOffset.Now;
        }

        private static DateTimeOffset UnixTimeStampToDateTime(double aUnixTimeStamp)
        {
            var lDateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, 0, TimeSpan.FromHours(0));
            return lDateTime.AddSeconds(aUnixTimeStamp);
        }

        internal readonly struct ValidationTokenResult
        {
            public ValidationTokenResult(ClaimsPrincipal aClaimsPrincipal, JwtSecurityToken aSecurityToken)
            {
                ClaimsPrincipal = aClaimsPrincipal;
                SecurityToken = aSecurityToken;
            }
            public readonly ClaimsPrincipal ClaimsPrincipal;
            public readonly JwtSecurityToken SecurityToken;
        }

    }
}

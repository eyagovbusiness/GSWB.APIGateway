using APIGateway.Application.DTOs;
using APIGateway.Domain.Entities;
using System.Collections.Immutable;
using System.Security.Claims;
using TGF.Common.ROP.HttpResult;

namespace APIGateway.Application
{
    /// <summary>
    /// Interface of the <see cref="TokenService"/> that provides all the methods needed to support a double token auth system with AccessToken and RefreshToken.   
    /// This features are: providing a new pair of AccessToken-RefreshToken, a method to refresh the AccessToken providing both the AccessToken-RefreshToken and finally giving support to revoke token pairs from the server side, forcing the user to ask for a new pair of tokens.
    /// </summary>
    public interface ITokenService
    {

        /// <summary>
        /// Generates a new <see cref="TokenPairDTO"/> asynchronously.
        /// </summary>
        /// <param name="aClaimsPrincipal"><see cref="ClaimsPrincipal"/> from the authenticated user by the PreAuthCookie.</param>
        /// <returns>A newly generated <see cref="TokenPairDTO"/> or null.</returns>
        Task<IHttpResult<TokenPairDTO>> GetNewTokenPairAsync(ClaimsPrincipal aClaimsPrincipal, CancellationToken aCancellationToken = default);

        /// <summary>
        /// Retrieves a refreshed access token asynchronously.Only outdated or expired tokens can be refreshed.
        /// </summary>
        /// <param name="aTokenPair">The current <see cref="TokenPairDTO"/>.</param>
        /// <returns>A refreshed access token or null.</returns>
        Task<IHttpResult<string>> GetRefreshedAccessTokenAsync(TokenPairDTO aTokenPair, CancellationToken aCancellationToken = default);

        /// <summary>
        /// Mark as outdated all the <see cref="TokenPairAuthRecord"/> related with any of the provided DiscordUser ID.
        /// </summary>
        /// <param name="aDiscordUserIdList">The list of members identified by the DiscordUser IDs whose tokens need to be revoked.</param>
        /// <returns>List of <see cref="string"/> with the revoked access tokens.</returns>
        Task<IHttpResult<ImmutableArray<string>>> OutdateTokenPairForMemberListAsync(IEnumerable<ulong> aDiscordUserIdList, CancellationToken aCancellationToken = default);

        /// <summary>
        /// Mark as outdated all the <see cref="TokenPairAuthRecord"/> related with any of the provided DiscordRole ID.
        /// </summary>
        /// <param name="aDiscordRoleIdList">List of DiscordRoleId used to revoke the tokens.</param>
        /// <returns>List of <see cref="string"/> with the revoked access tokens.</returns>
        Task<IHttpResult<ImmutableArray<string>>> OutdateTokenPairForRoleListAsync(IEnumerable<ulong> aDiscordRoleIdList, CancellationToken aCancellationToken = default);
    }
}

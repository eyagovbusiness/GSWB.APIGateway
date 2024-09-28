using APIGateway.Domain.Entities;
using System.Collections.Immutable;
using TGF.CA.Domain.Contracts.Repositories;
using TGF.Common.ROP;
using TGF.Common.ROP.HttpResult;

namespace APIGateway.Application
{
    /// <summary>
    /// Represents operations that can be performed on a repository of token pairs for authentication records.
    /// </summary>
    /// <remarks>Serves for IoC with the Infrastructure layer(which now depends on the Application layer)</remarks>
    public interface ITokenPairAuthRecordRepository: IRepositoryBase<TokenPairAuthRecord, Guid>
    {

        /// <summary>
        /// Retrieves a <see cref="TokenPairAuthRecord"/> by its refresh token.
        /// </summary>
        /// <param name="aRefreshToken">The refresh token.</param>
        /// <param name="aCancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The <see cref="TokenPairAuthRecord"/> with the specified RefreshToken or Error.</returns>
        Task<IHttpResult<TokenPairAuthRecord>> GetByRefreshTokenAsync(string aRefreshToken, CancellationToken aCancellationToken = default);

        /// <summary>
        /// Revokes the tokens for a list of DiscordUser IDs.
        /// </summary>
        /// <param name="aDiscordUserIdList">The list of DiscordUser IDs.</param>
        /// <param name="aCancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The list of AccessTokens revoked or Error.</returns>
        Task<IHttpResult<ImmutableArray<string>>> RevokeByDiscordUserIdListAsync(IEnumerable<ulong> aDiscordUserIdList, CancellationToken aCancellationToken = default);

        /// <summary>
        /// Revokes the tokens for a list of DiscordRole IDs.
        /// </summary>
        /// <param name="aDiscordRoleIdList">The list of DiscordRole IDs.</param>
        /// <param name="aCancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The list of AccessTokens revoked or Error.</returns>
        Task<IHttpResult<ImmutableArray<string>>> RevokeByDiscordRoleIdListAsync(IEnumerable<ulong> aDiscordRoleIdList, CancellationToken aCancellationToken = default);

        /// <summary>
        /// Deletes all expired records.
        /// </summary>
        /// <param name="aCancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The number of records deleted or Error.</returns>
        Task<IHttpResult<int>> DeleteExpiredRecordsAsync(CancellationToken aCancellationToken = default);

        /// <summary>
        /// Deletes a given token pair record by its refresh token.
        /// </summary>
        /// <param name="aRefreshToken">The refresh token used to select the TokenPair row to be deleted from db.</param>
        /// <param name="aCancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>ROP Result.</returns>
        Task<IHttpResult<Unit>> DeleteByRefreshTokenAsync(string aRefreshToken, CancellationToken aCancellationToken = default);

    }

}

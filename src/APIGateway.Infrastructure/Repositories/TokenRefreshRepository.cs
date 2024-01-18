using APIGateway.Application;
using APIGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using TGF.CA.Infrastructure.DB.Repository;
using TGF.Common.ROP.HttpResult;
using TGF.Common.ROP.Result;

namespace APIGateway.Infrastructure
{

    /// <summary>
    /// Implementation of <see cref="ITokenPairAuthRecordRepository"/> that operates over the <see cref="APIGatewayAuthDbContext"/> database context.
    /// </summary>
    internal class TokenPairAuthRecordRepository : RepositoryBase<TokenPairAuthRecordRepository, APIGatewayAuthDbContext>, ITokenPairAuthRecordRepository
    {
        public TokenPairAuthRecordRepository(APIGatewayAuthDbContext aContext, ILogger<TokenPairAuthRecordRepository> aLogger)
        : base(aContext, aLogger) { }

        #region ITokenPairAuthRecordRepository
        public async Task<IHttpResult<TokenPairAuthRecord>> GetByRefreshTokenAsync(string aRefreshToken, CancellationToken aCancellationToken = default)
            => await TryQueryAsync((aCancellationToken) => _context.TokenPairAuthRecords.FirstAsync(t => t.RefreshToken == aRefreshToken, aCancellationToken), aCancellationToken);

        public async Task<IHttpResult<TokenPairAuthRecord>> AddAsync(TokenPairAuthRecord aTokenPairAuthRecord, CancellationToken aCancellationToken = default)
            => await base.AddAsync(aTokenPairAuthRecord, aCancellationToken);

        public async Task<IHttpResult<TokenPairAuthRecord>> UpdateAsync(TokenPairAuthRecord aTokenPairAuthRecord, CancellationToken aCancellationToken = default)
            => await base.UpdateAsync(aTokenPairAuthRecord, aCancellationToken);

        public async Task<IHttpResult<ImmutableArray<string>>> RevokeByDiscordUserIdListAsync(IEnumerable<ulong> aDiscordUserIdList, CancellationToken aCancellationToken = default)
         => await RevokeTokenListAsync(await _context.TokenPairAuthRecords
            .Where(tokensRecord => aDiscordUserIdList.Contains(tokensRecord.DiscordUserId) && !tokensRecord.IsOutdated)
            .ToListAsync(aCancellationToken), aCancellationToken);

        public async Task<IHttpResult<ImmutableArray<string>>> RevokeByDiscordRoleIdListAsync(IEnumerable<ulong> aDiscordRoleIdList, CancellationToken aCancellationToken = default)
        => await RevokeTokenListAsync(await _context.TokenPairAuthRecords
            .Where(tokensRecord => aDiscordRoleIdList.Contains(tokensRecord.DiscordRoleId) && !tokensRecord.IsOutdated)
            .ToListAsync(aCancellationToken), aCancellationToken);

        public async Task<IHttpResult<int>> DeleteExpiredRecordsAsync(CancellationToken aCancellationToken = default)
        => await TryCommandAsync(() =>
        {
            var expiredRecords = _context.TokenPairAuthRecords
                .Where(t => t.ExpiryDate < DateTime.UtcNow);
            _context.TokenPairAuthRecords.RemoveRange(expiredRecords);
            return 0;
        }
        , aCancellationToken
        , aSaveResultOverride: (int aChangeCount, int aCommandResult)
        => Result.SuccessHttp(aChangeCount));

        #endregion

        #region Private 
        private async Task<IHttpResult<ImmutableArray<string>>> RevokeTokenListAsync(IEnumerable<TokenPairAuthRecord> aTokenPairAuthRecordList, CancellationToken aCancellationToken = default)
        => await TryCommandAsync(() =>
        {
            var lTokenListToRevoke = aTokenPairAuthRecordList.ToArray();

            if (!lTokenListToRevoke.Any())
                return ImmutableArray<string>.Empty;
            foreach (var lRcord in lTokenListToRevoke)
                lRcord.IsOutdated = true;

            return lTokenListToRevoke.Select(t => t.AccessToken).ToImmutableArray();
        }
        , aCancellationToken
        , aSaveResultOverride: (int aChangeCount, ImmutableArray<string> aCommandResult)
        => aChangeCount >= aCommandResult.Length
            ? Result.SuccessHttp(aCommandResult)
            : Result.Failure<ImmutableArray<string>>(InfrastructureErrors.AuthDatabase.NotAllTokenRevocationSaved));

        #endregion

    }

}

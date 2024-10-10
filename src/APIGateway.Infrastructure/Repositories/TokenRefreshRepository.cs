using APIGateway.Application;
using APIGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using TGF.CA.Infrastructure.DB.Repository;
using TGF.Common.ROP;
using TGF.Common.ROP.HttpResult;
using TGF.Common.ROP.Result;

namespace APIGateway.Infrastructure
{

    /// <summary>
    /// Implementation of <see cref="ITokenPairAuthRecordRepository"/> that operates over the <see cref="AuthDbContext"/> database context.
    /// </summary>
    internal class TokenPairAuthRecordRepository
        (AuthDbContext aContext, ILogger<TokenPairAuthRecordRepository> aLogger) 
        : RepositoryBase<TokenPairAuthRecordRepository, AuthDbContext, TokenPairAuthRecord, Guid>(aContext, aLogger), ITokenPairAuthRecordRepository
    {

        #region ITokenPairAuthRecordRepository
        public async Task<IHttpResult<TokenPairAuthRecord>> GetByRefreshTokenAsync(string aRefreshToken, CancellationToken aCancellationToken = default)
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            => await TryQueryAsync((aCancellationToken) => _context.TokenPairAuthRecords.FirstOrDefaultAsync(t => t.RefreshToken == aRefreshToken, aCancellationToken), aCancellationToken)
            .Verify(tokenPairAuthRecord => tokenPairAuthRecord! != default!, InfrastructureErrors.AuthDatabase.RefreshTokenNotFound);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

        public async Task<IHttpResult<ImmutableArray<string>>> RevokeByDiscordUserIdListAsync(IEnumerable<Guid> memberIdList, CancellationToken aCancellationToken = default)
         => await RevokeTokenListAsync(await _context.TokenPairAuthRecords
            .Where(tokensRecord => memberIdList.Contains(tokensRecord.MemberId) && !tokensRecord.IsOutdated)
            .ToListAsync(aCancellationToken), aCancellationToken);

        public async Task<IHttpResult<ImmutableArray<string>>> RevokeByDiscordRoleIdListAsync(IEnumerable<ulong> aDiscordRoleIdList, CancellationToken aCancellationToken = default)
        => await RevokeTokenListAsync(await _context.TokenPairAuthRecords
            .Where(tokensRecord => aDiscordRoleIdList.Contains(tokensRecord.DiscordRoleId) && !tokensRecord.IsOutdated)
            .ToListAsync(aCancellationToken), aCancellationToken);

        public async Task<IHttpResult<int>> DeleteExpiredRecordsAsync(CancellationToken aCancellationToken = default)
        => await TryCommandAsync(() =>
        {
            var expiredRecords = _context.TokenPairAuthRecords
                .Where(t => t.ExpiryDate < DateTimeOffset.Now);
            _context.TokenPairAuthRecords.RemoveRange(expiredRecords);
            return 0;
        }
        , aCancellationToken
        , aSaveResultOverride: (int aChangeCount, int aCommandResult)
        => Result.SuccessHttp(aChangeCount));

        public async Task<IHttpResult<Unit>> DeleteByRefreshTokenAsync(string aRefreshToken, CancellationToken aCancellationToken = default)
        => await TryCommandAsync(() =>
        {
            var lTokenRecordToDelete = _context.TokenPairAuthRecords
                .FirstOrDefault(t => t.RefreshToken == aRefreshToken);

            if (lTokenRecordToDelete! != null!)
                _context.TokenPairAuthRecords.Remove(lTokenRecordToDelete!);

            return Unit.Value;
        }
        , aCancellationToken
        , aSaveResultOverride: (int aChangeCount, Unit aCommandResult)
        => aChangeCount > 0
            ? Result.SuccessHttp(aCommandResult)
            : Result.Failure<Unit>(InfrastructureErrors.AuthDatabase.RefreshTokenNotFound));

        #endregion

        #region Private 
        private async Task<IHttpResult<ImmutableArray<string>>> RevokeTokenListAsync(IEnumerable<TokenPairAuthRecord> aTokenPairAuthRecordList, CancellationToken aCancellationToken = default)
        => await TryCommandAsync(() =>
        {
            var lTokenListToRevoke = aTokenPairAuthRecordList.ToArray();

            if (lTokenListToRevoke.Length == 0)
                return [];
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

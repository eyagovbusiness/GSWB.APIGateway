using APIGateway.Application;
using Common.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace APIGateway.Infrastructure.Services
{
    /// <summary>
    /// Service to perform token revocation operations(revoking refresh tokens in authDB and balck-listing access tokens). This includes revoking tokens for a list of members and checking if a given access token has been revoked.
    /// </summary>
    internal class TokenRevocationService : ITokenRevocationService, IDisposable
    {
        private const int _numberOfTokenBuckets = 3;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private readonly TimeSpan _tokenLifespan;

        private readonly HashSet<string>[] _revokedTokenBuckets = new HashSet<string>[_numberOfTokenBuckets];
        private readonly DateTimeOffset[] _revocationBucketTimes = new DateTimeOffset[_numberOfTokenBuckets];
        private readonly Timer[] _purgeBucketTimers = new Timer[_numberOfTokenBuckets];

        public TokenRevocationService(IServiceScopeFactory aServiceScopeFactory, ILogger<TokenRevocationService> aLogger)
        {
            _serviceScopeFactory = aServiceScopeFactory;
            _logger = aLogger;
            _tokenLifespan = DefaultTokenLifetimes.AccessToken;

            SetUpTokenBuckets();
        }

        #region ITokenRevocationService

        public bool IsAccessTokenRevoked(string aJwtToken)
        {
            for (int i = 0; i < _revokedTokenBuckets.Length; i++)
                if (_revokedTokenBuckets[i].Contains(aJwtToken))
                    return true;
            return false;
        }

        public async Task OutdateByDiscordUserListAsync(ulong[] aDiscordUserIdList, CancellationToken aCancellationToken)
        {
            ImmutableArray<string> lListOfRevokedTokens = Array.Empty<string>().ToImmutableArray();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var lTokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                var lRevocationResult = await lTokenService.OutdateTokenPairForMemberListAsync(aDiscordUserIdList, aCancellationToken);
                if (!lRevocationResult.IsSuccess)
                    throw new Exception("Error revoking tokens in DB, security may be compromised!!");

                lListOfRevokedTokens = lRevocationResult.Value;
            }
            if (lListOfRevokedTokens.Length > 0)
                UpdateRevokedTokenBuckets(lListOfRevokedTokens);
        }

        public async Task OutdateByDiscordRoleListAsync(ulong[] aDiscordRoleIdList, CancellationToken aCancellationToken)
        {
            ImmutableArray<string> lListOfRevokedTokens = Array.Empty<string>().ToImmutableArray();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var lTokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                var lRevocationResult = await lTokenService.OutdateTokenPairForRoleListAsync(aDiscordRoleIdList, aCancellationToken);
                if (!lRevocationResult.IsSuccess)
                    throw new Exception("Error revoking tokens in DB, security may be compromised!!");

                lListOfRevokedTokens = lRevocationResult.Value;
            }
            if (lListOfRevokedTokens.Length > 0)
                UpdateRevokedTokenBuckets(lListOfRevokedTokens);
        }

        #endregion

        #region Private

        /// <summary>
        /// Updates the black-listed token list due to server revocation.
        /// </summary>
        /// <param name="aListOfRevokedTokens">List of revoked tokens.</param>
        private void UpdateRevokedTokenBuckets(IEnumerable<string> aListOfRevokedTokens)
        {
            try
            {
                // First, try to find an empty HashSet or the oldest one if none are empty
                int lTargetBucketIndex = -1;
                for (int i = 0; i < _revokedTokenBuckets.Length; i++)
                {
                    if (_revokedTokenBuckets[i].Count == 0)
                    {
                        lTargetBucketIndex = i;
                        break;
                    }
                }

                // If none are empty, just use the last one and update its time
                if (lTargetBucketIndex == -1)
                    lTargetBucketIndex = _revokedTokenBuckets.Length - 1;

                // Update the time and tokens for the target index
                _revocationBucketTimes[lTargetBucketIndex] = DateTimeOffset.Now;
                _revokedTokenBuckets[lTargetBucketIndex].UnionWith(aListOfRevokedTokens);
            }
            catch (Exception lEx)
            {
                _logger.LogCritical(lEx, "Critical error occurred while trying to revoke tokens.");
            }
        }

        /// <summary>
        /// Callback method for the scheduled token bucket cleanup tasks. This method clears the token bucket by its index if the contained tokens have expired.
        /// </summary>
        /// <param name="aState">object with the <see cref="int"/> index of the current token bucket from <see cref="_revokedTokenBuckets"/> </param>
        private void PurgeCallback(object? aState)
        {
            int lBucketIndex = (int)aState!;
            if (_revocationBucketTimes[lBucketIndex].Add(_tokenLifespan) < DateTimeOffset.Now)
                _revokedTokenBuckets[lBucketIndex].Clear();
        }

        /// <summary>
        /// Setup the token buckets to empty hash sets and schedules the cleanup tasks for each bucket.
        /// </summary>
        private void SetUpTokenBuckets()
        {
            int lPurgeIntervalMilliseconds = (int)_tokenLifespan.TotalMilliseconds + 1000;
            for (int i = 0; i < _revokedTokenBuckets.Length; i++)
            {
                _revokedTokenBuckets[i] = new HashSet<string>();
                _purgeBucketTimers[i] = new Timer(PurgeCallback, i, lPurgeIntervalMilliseconds, lPurgeIntervalMilliseconds);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            foreach (var lTimer in _purgeBucketTimers)
                lTimer?.Dispose();
        }

        #endregion

    }
}

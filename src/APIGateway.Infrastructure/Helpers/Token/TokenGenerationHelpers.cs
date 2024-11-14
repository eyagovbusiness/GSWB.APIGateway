using APIGateway.Domain.Entities;
using Common.Application.Contracts.Services;
using Common.Application.DTOs.Auth;
using Common.Application.DTOs.Members;
using Common.Application.DTOs.ProcessingHelpers;
using Common.Domain.ValueObjects;
using Common.Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using TGF.CA.Application;
using TGF.Common.ROP.HttpResult;
using TGF.Common.ROP.Result;
using Result = TGF.Common.ROP.Result.Result;
using ROPResult = TGF.Common.ROP.Result;
using TGF.Common.ROP.HttpResult.RailwaySwitches;

namespace APIGateway.Infrastructure.Helpers.Token
{
    public static class TokenGenerationHelpers
    {
        public const int RefreshTokenByteLenght = 64;
        public static int RefreshTokenLength = 4 * ((RefreshTokenByteLenght + 2) / 3);


        /// <summary>
        /// Get the DiscordCookie claims.
        /// </summary>
        /// <param name="aClaimsPrincipal">List of claims from the authenticated discord user from the discord cookie.</param>
        /// <returns><see cref="DiscordCookieUserInfo"/> with all the authenticated discord user data.</returns>
        public static DiscordCookieUserInfo GetDiscordCookieUserInfo(ClaimsPrincipal aClaimsPrincipal)
        => new(aClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!,
                aClaimsPrincipal.FindFirstValue(ClaimTypes.Name)!,
                aClaimsPrincipal.FindFirstValue(ClaimTypes.GivenName)!);


        /// <summary>
        /// Generates a random RefreshToken that will be paired with an AccessToken and will be required to refresh this last one.
        /// </summary>
        /// <returns><see cref="string"/> representing the RefreshToken.</returns>
        internal static string GenerateRefreshToken()
        {
            var lRandomBytes = new byte[RefreshTokenByteLenght];
            using var lRng = RandomNumberGenerator.Create();
            lRng.GetBytes(lRandomBytes);
            return Convert.ToBase64String(lRandomBytes);
        }

        /// <summary>
        /// Get a new list of claims for the provided member <see cref="aMemberDTO"/> and the discordCookie data <see cref="aDiscordCookieUserInfo"/>.
        /// </summary>
        /// <returns>List of claims related to the authenticated member.</returns>
        internal static IHttpResult<IEnumerable<Claim>> GetNewClaims(DiscordCookieUserInfo aDiscordCookieUserInfo, MemberDetailDTO aMemberDetailDTO, string? aIssuer = default, string? aAudience = default)
        {
            Claim lPermissionsClaim = default!;
            return GetPermissionsClaim(aMemberDetailDTO)
                .Tap(permissionsClaim => lPermissionsClaim = permissionsClaim)
                .Bind(_ => GetRoleClaim(aMemberDetailDTO))
                .Map(roleClaim =>
                    new List<Claim>
                    {
                        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new(ClaimTypes.NameIdentifier, aDiscordCookieUserInfo.UserNameIdentifier),
                        new(GuildSwarmClaims.GuildId,aMemberDetailDTO.GuildId),
                        new(ClaimTypes.Name, aDiscordCookieUserInfo.UserName),
                        new(ClaimTypes.GivenName, aDiscordCookieUserInfo.GivenName ?? string.Empty),
                        new(GuildSwarmClaims.IssuerClaimType, aIssuer ?? string.Empty),
                        new(GuildSwarmClaims.AudienceClaimType, aAudience ?? string.Empty),
                        roleClaim,
                        lPermissionsClaim!
                    } as IEnumerable<Claim>
                );
        }

        /// <summary>
        /// Take the provided <see cref="aClaimsPrincipal"/> of a given member and updates the permissions claim if they were outdated.
        /// </summary>
        /// <param name="aMembersCommunicationService">Service to communicate directly via http calls with the members microservice.</param>
        /// <param name="aClaimsPrincipal">The list of claims related with the provided <see cref="aTokenPairAuthRecord"/>.</param>
        /// <param name="aTokenPairAuthRecord">Token pair autDB record with the access token related to the provided claims(used to check if the record is outdated).</param>
        /// <returns>Result with the updated list of claims.</returns>
        internal static async Task<IHttpResult<IEnumerable<Claim>>> GetUpdatedClaims(IMembersCommunicationService aMembersCommunicationService, ClaimsPrincipal aClaimsPrincipal, TokenPairAuthRecord aTokenPairAuthRecord, CancellationToken aCancellationToken = default)
        => await Result.CancellationTokenResult(aCancellationToken)
        .Bind(async _ =>
        {
            var lClaimList = aClaimsPrincipal.Claims.ToList();
            if (aTokenPairAuthRecord.IsOutdated)
            {
                MemberDetailDTO lMemberDTO = default!;
                Claim lPermissionsClaim = default!;
                return await aMembersCommunicationService
                .GetExistingMember(new MemberKey(lClaimList.FirstOrDefault(x => x.Type == GuildSwarmClaims.GuildId)!.Value, lClaimList.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value), aCancellationToken)
                .Tap(memberDTO => lMemberDTO = memberDTO)
                .Bind(memberDTO => GetPermissionsClaim(memberDTO))
                .Tap(permissionClaim => lPermissionsClaim = permissionClaim)
                .Bind(_ => GetRoleClaim(lMemberDTO))
                .Map(roleClaim =>
                {
                    var lOutdatedPermissionsClaim = lClaimList.FirstOrDefault(x => x.Type == DefaultApplicationClaimTypes.Permissions);
                    lClaimList.Remove(lOutdatedPermissionsClaim!);
                    lClaimList.Add(lPermissionsClaim);

                    var lOutdatedRoleClaim = lClaimList.FirstOrDefault(x => x.Type == ClaimTypes.Role);
                    lClaimList.Remove(lOutdatedRoleClaim!);
                    lClaimList.Add(roleClaim);
                    return lClaimList as IEnumerable<Claim>;
                });
            }
            return Result.SuccessHttp(lClaimList as IEnumerable<Claim>);
        });

        /// <summary>
        /// Get the member's permissions claim.
        /// </summary>
        /// <param name="aMemberDTO">Member from who to obtain the permissios claim.</param>
        /// <returns>Member's permissions claim.</returns>
        internal static IHttpResult<Claim> GetPermissionsClaim(MemberDetailDTO aMemberDetailDTO)
        {
            if (aMemberDetailDTO.Status == MemberStatusEnum.Banned)
                return ROPResult.Result.SuccessHttp<Claim>(new(DefaultApplicationClaimTypes.Permissions, ((int)PermissionsEnum.None).ToString()));

            var lHighestApplicationRole = aMemberDetailDTO.GetHighestRole();
            return lHighestApplicationRole != null
                ? ROPResult.Result.SuccessHttp<Claim>(new(DefaultApplicationClaimTypes.Permissions, ((int)lHighestApplicationRole.Permissions).ToString()))
                : ROPResult.Result.Failure<Claim>(InfrastructureErrors.Identity.Claims.NoApplicationRoleFound);
        }

        /// <summary>
        /// Get the member's Role claim.
        /// </summary>
        /// <param name="aMemberDTO">Member from who to obtain the Role claim.</param>
        /// <returns>Member's Role claim.</returns>
        internal static IHttpResult<Claim> GetRoleClaim(MemberDetailDTO aMemberDetailDTO)
        {
            var lHighestApplicationRole = aMemberDetailDTO.GetHighestRole();
            return lHighestApplicationRole != null
                ? ROPResult.Result.SuccessHttp<Claim>(new(ClaimTypes.Role, lHighestApplicationRole.Id))
                : ROPResult.Result.Failure<Claim>(InfrastructureErrors.Identity.Claims.NoApplicationRoleFound);
        }

    }
}

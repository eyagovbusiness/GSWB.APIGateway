using Common.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using TGF.CA.Domain.Primitives;

namespace APIGateway.Domain.Entities
{
    /// <summary>
    /// Represents a database record to persist access-refresh auth token pairs and support token revocation.
    /// </summary
    /// <remarks><see cref="MemberId"/> and <see cref="RoleId"/> have the the GuildId property wich are both mapped to the same single column in DB, so it has the same value always for both.</remarks>
    public class TokenPairAuthRecord : Entity<Guid>
    {

        /// <summary>
        /// The authenticated member's id which generated this token pair.
        /// </summary>
        public MemberKey MemberId { get; init; }

        /// <summary>
        /// The Id of highest DiscordRole in the role's hierarchy(with RoleType = Application) which was givin the permissions to this token when it was created.
        /// </summary>
        public RoleKey RoleId { get; init; }

        /// <summary>
        /// The expiry date for the Refresh token, therefore the pair record itself. 
        /// </summary>
        public DateTimeOffset ExpiryDate { get; set; }

        /// <summary>
        /// The refresh token required to refresh the associated access token.
        /// </summary>

        [MaxLength(88)]//64-byte random number and then converting it to a Base64 string always 88 long
        public required string RefreshToken { get; set; }

        /// <summary>
        /// The access token used for endpoint authorization.
        /// </summary>
        [MaxLength(2048)]
        public required string AccessToken { get; set; }

        /// <summary>
        /// Whether or not the access token of this record is outdated, meaning a new access token needs to be generated for this record instead of just extending the ExpiryDate.
        /// </summary>
        public bool IsOutdated { get; set; }

        internal TokenPairAuthRecord() { MemberId = default!; RoleId = default!; }
        public TokenPairAuthRecord(MemberKey memberId, RoleKey roleId) { MemberId = memberId; RoleId = roleId; }

    }
}

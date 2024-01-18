using System.ComponentModel.DataAnnotations;

namespace APIGateway.Domain.Entities
{
    /// <summary>
    /// Represents a database record to persist access-refresh auth token pairs and support token revocation.
    /// </summary>
    public class TokenPairAuthRecord
    {
        /// <summary>
        /// Global unique id.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// The authenticated discord user's id which generated this token pair.
        /// </summary>
        public ulong DiscordUserId { get; set; }

        /// <summary>
        /// The Id of highest DiscordRole in the role's hierarchy(with RoleType = Application) which was givin the permissions to this token when it was created.
        /// </summary>
        public ulong DiscordRoleId { get; set; }

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

    }
}

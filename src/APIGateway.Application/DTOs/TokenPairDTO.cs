
namespace APIGateway.Application.DTOs
{
    /// <summary>
    /// Record to work at the application level with the pair AccessToken-RefreshToken strings.
    /// </summary>
    /// <param name="AccessToken">AccessToken string.</param>
    /// <param name="RefreshToken">RefreshToken string.</param>
    /// <remarks>Expected from the request body an equivalent JSON when requesting token refresh.</remarks>
    public record TokenPairDTO(string AccessToken, string RefreshToken);
}

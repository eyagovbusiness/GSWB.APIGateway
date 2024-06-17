using TGF.Common.ROP.HttpResult;
using APIGateway.Domain.Entities;

namespace APIGateway.Application.Contracts.Services
{
    /// <summary>
    /// Service to implement the use case to get an existing ConsentLog and by the way update it if the request ip address doesnot match with the registered ip address.
    /// </summary>
    /// <remarks>We asume if the client knows the ConsentLog Id is because the user is the owner of the consent log itself since it is the only way to get it in the frontend.</remarks>
    public interface IGetConsentLegalService
    {
        /// <summary>
        /// Get an existing ConsentLog and by the way update it if the request ip address doesnot match with the registered ip address.
        /// </summary>
        /// <param name="aUserIpAddress">Ip address of the user retrieved from the consent http request.</param>
        /// <param name="aConsentLogId">The ConsentLog id.</param>
        /// <param name="aCancellationToken"></param>
        /// <returns></returns>
        Task<IHttpResult<ConsentLog>> GetConsentLegal(string aUserIpAddress, Guid aConsentLogId, CancellationToken aCancellationToken);
    }
}

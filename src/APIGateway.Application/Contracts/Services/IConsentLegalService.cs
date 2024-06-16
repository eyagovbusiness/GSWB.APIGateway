using Common.Application.DTOs.Legal;
using TGF.Common.ROP.HttpResult;
using APIGateway.Domain.Entities;

namespace APIGateway.Application.Contracts.Services
{
    /// <summary>
    /// Service to implement the use case to create a new ConsentLog
    /// </summary>
    public interface IConsentLegalService
    {
        /// <summary>
        /// Registers a new user consent log.
        /// </summary>
        /// <param name="aUserIpAddress">Ip address of the user retrieved from the consent http request.</param>
        /// <param name="aConsentLogDto">The information sent by the client about the consent given.</param>
        /// <param name="aCancellationToken"></param>
        /// <returns></returns>
        Task<IHttpResult<ConsentLog>> ConsentLegal(string aUserIpAddress, ConsentLogDTO aConsentLogDto, CancellationToken aCancellationToken);
    }
}

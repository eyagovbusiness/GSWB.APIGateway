using APIGateway.Application.Contracts.Services;
using APIGateway.Domain.Entities;
using TGF.CA.Application;
using TGF.Common.ROP.HttpResult;
using TGF.Common.ROP.Result;

namespace APIGateway.Application.UseCases
{
    public class GetConsentLegalService(
        IConsentLogRepository aConsentLogRepository,
        IEncryptionService aEncryptionService) 
    : IGetConsentLegalService
    {
        public async Task<IHttpResult<ConsentLog>> GetConsentLegal(string aUserIpAddress, Guid aConsentLogId, CancellationToken aCancellationToken)
            => await aConsentLogRepository.GetByIdAsync(aConsentLogId, aCancellationToken)
            .Bind(consentLog => KeepConsentLogIpUpdated(aUserIpAddress, consentLog, aCancellationToken));

        private async Task<IHttpResult<ConsentLog>> KeepConsentLogIpUpdated(string aUserIpAddress, ConsentLog aConsentLog, CancellationToken aCancellationToken)
        {
            var lEncryptedUserIpAddress = await aEncryptionService.EncryptAsync(aUserIpAddress);
            if(aConsentLog.IpAddress != lEncryptedUserIpAddress)
            {
                aConsentLog.IpAddress = lEncryptedUserIpAddress;
               return await aConsentLogRepository.UpdateAsync(aConsentLog,aCancellationToken);
            }
            return Result.SuccessHttp(aConsentLog);

        }
    }
}

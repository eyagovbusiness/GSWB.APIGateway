using APIGateway.Application;
using APIGateway.Domain.Entities;
using Microsoft.Extensions.Logging;
using TGF.CA.Infrastructure.DB.Repository;
using TGF.Common.ROP.HttpResult;

namespace APIGateway.Infrastructure.Repositories
{
    public class ConsentLogRepository
        (AuthDbContext aContext, ILogger<ConsentLogRepository> aLogger)
         : RepositoryBase<ConsentLogRepository, AuthDbContext>(aContext, aLogger), IConsentLogRepository
    {

        public Task<IHttpResult<ConsentLog>> AddAsync(ConsentLog aConsentLog, CancellationToken aCancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IHttpResult<ConsentLog>> DeleteAsync(Guid aConsentLogId, CancellationToken aCancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IHttpResult<ConsentLog>> UpdateAsync(ConsentLog aConsentLog, CancellationToken aCancellationToken = default)
        {
            throw new NotImplementedException();
        }

        //public ConsentLog GetAsync(Guid id)
        //{
        //    var consentLog = _context.ConsentLogs.Find(id);
        //    if (consentLog != null)
        //    {
        //        consentLog.IpAddress = aEncryptionService.Decrypt(consentLog.IpAddress);
        //    }
        //    return consentLog;
        //}

        //public void AddAsync(ConsentLog consentLog)
        //{
        //    consentLog.IpAddress = aEncryptionService.Encrypt(consentLog.IpAddress);
        //    _context.ConsentLogs.Add(consentLog);
        //    _context.SaveChanges();
        //}

    }
}

using APIGateway.Application;
using APIGateway.Domain.Entities;
using Microsoft.Extensions.Logging;
using TGF.CA.Infrastructure.DB.Repository;
using TGF.Common.ROP.HttpResult;

namespace APIGateway.Infrastructure.Repositories
{
    public class ConsentLogRepository
        (LegalDbContext aContext, ILogger<ConsentLogRepository> aLogger)
         : RepositoryBase<ConsentLogRepository, LegalDbContext>(aContext, aLogger), IConsentLogRepository
    {
        public async Task<IHttpResult<ConsentLog>> GetByIdAsync(Guid aConsentLogId, CancellationToken aCancellationToken = default)
        => await base.GetByIdAsync<ConsentLog, Guid>(aConsentLogId, aCancellationToken);
        public async Task<IHttpResult<ConsentLog>> AddAsync(ConsentLog aConsentLog, CancellationToken aCancellationToken = default)
        => await base.AddAsync(aConsentLog, aCancellationToken);
        public async Task<IHttpResult<ConsentLog>> UpdateAsync(ConsentLog aConsentLog, CancellationToken aCancellationToken = default)
        => await base.UpdateAsync(aConsentLog, aCancellationToken);

    }
}

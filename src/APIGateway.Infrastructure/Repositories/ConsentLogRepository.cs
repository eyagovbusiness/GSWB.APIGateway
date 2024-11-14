using APIGateway.Application;
using APIGateway.Domain.Entities;
using Microsoft.Extensions.Logging;
using TGF.CA.Infrastructure.DB.Repository;
using TGF.CA.Infrastructure.DB.Repository.CQRS.EntityRepository;

namespace APIGateway.Infrastructure.Repositories
{
    public class ConsentLogRepository
        (LegalDbContext aContext, ILogger<ConsentLogRepository> aLogger)
         : EntityRepository<ConsentLogRepository, LegalDbContext, ConsentLog, Guid>(aContext, aLogger), IConsentLogRepository
    {

    }
}

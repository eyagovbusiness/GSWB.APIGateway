using APIGateway.Domain.Entities;
using TGF.CA.Domain.Contracts.Repositories.EntityRepository;

namespace APIGateway.Application
{
    /// <summary>
    /// Repository to work with the ConsentLog entities in the LegalDb
    /// </summary>
    public interface IConsentLogRepository : IEntityRepository<ConsentLog, Guid>
    {

    }

}

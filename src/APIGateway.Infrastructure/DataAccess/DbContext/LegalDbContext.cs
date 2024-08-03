using APIGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TGF.CA.Infrastructure.DB.DbContext;

namespace APIGateway.Infrastructure
{
    public class LegalDbContext(DbContextOptions<LegalDbContext> aOptions) : EntitiesDbContext<LegalDbContext>(aOptions)
    {
        public DbSet<ConsentLog> ConsentLogs { get; set; }

    }
}

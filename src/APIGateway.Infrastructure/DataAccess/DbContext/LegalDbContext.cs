using APIGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIGateway.Infrastructure
{
    public class LegalDbContext(DbContextOptions<LegalDbContext> aOptions) : DbContext(aOptions)
    {
        public DbSet<ConsentLog> ConsentLogs { get; set; }
    }
}

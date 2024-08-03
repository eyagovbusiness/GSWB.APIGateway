using APIGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TGF.CA.Domain.Primitives;
using TGF.CA.Infrastructure.DB.DbContext;

namespace APIGateway.Infrastructure
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> aOptions) : EntitiesDbContext<AuthDbContext>(aOptions)
    {
        public DbSet<TokenPairAuthRecord> TokenPairAuthRecords { get; set; }

    }
}

using APIGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIGateway.Infrastructure
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> aOptions) : DbContext(aOptions)
    {
        public DbSet<TokenPairAuthRecord> TokenPairAuthRecords { get; set; }
    }
}

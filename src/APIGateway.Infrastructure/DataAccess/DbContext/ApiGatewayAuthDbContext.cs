using APIGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIGateway.Infrastructure
{
    public class APIGatewayAuthDbContext : DbContext
    {
        public APIGatewayAuthDbContext(DbContextOptions<APIGatewayAuthDbContext> options) : base(options) { }

        public DbSet<TokenPairAuthRecord> TokenPairAuthRecords { get; set; }
    }
}

using APIGateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TGF.CA.Infrastructure.DB.DbContext;

namespace APIGateway.Infrastructure
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> aOptions) : EntitiesDbContext<AuthDbContext>(aOptions)
    {
        public DbSet<TokenPairAuthRecord> TokenPairAuthRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the ValueConverter to convert ulong to decimal and back
            var ulongToDecimalConverter = new ValueConverter<ulong, decimal>(
                v => Convert.ToDecimal(v),    // Convert ulong to decimal for storage
                v => Convert.ToUInt64(v)      // Convert decimal back to ulong
            );

            modelBuilder.Entity<TokenPairAuthRecord>(entity =>
            {
                // Configure MemberKey as an owned type, mapping its properties to columns in the table
                entity.OwnsOne(e => e.MemberId, key =>
                {
                    key.Property(k => k.GuildId)
                        .HasColumnType("numeric(20,0)")
                        .HasConversion(ulongToDecimalConverter);  // Convert ulong to decimal

                    key.Property(k => k.UserId)
                        .HasColumnType("numeric(20,0)")
                        .HasConversion(ulongToDecimalConverter);  // Convert ulong to decimal
                });

                // Configure RoleKey as an owned type, mapping its properties to columns in the table
                entity.OwnsOne(e => e.RoleId, key =>
                {
                    key.Property(k => k.GuildId)
                        .HasColumnType("numeric(20,0)")
                        .HasConversion(ulongToDecimalConverter);  // Convert ulong to decimal

                    key.Property(k => k.RoleId)
                        .HasColumnType("numeric(20,0)")
                        .HasConversion(ulongToDecimalConverter);  // Convert ulong to decimal
                });

            });

        }


    }
}

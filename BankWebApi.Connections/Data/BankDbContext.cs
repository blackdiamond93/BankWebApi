using BankWebApi.Connections.Models;
using Microsoft.EntityFrameworkCore;

namespace BankWebApi.Connections.Data
{
    public class BankDbContext : DbContext
    {
        public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
        {

        }
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(c => c.DateOfBirth)
                      .IsRequired();
                entity.Property(c => c.Income)
                      .IsRequired()
                      .HasPrecision(18,2);
                entity.Property(c => c.Gender)
                      .IsRequired()
                      .HasMaxLength(10);

            });
            
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => a.AccountNumber)
                      .IsUnique();
                entity.HasOne(a => a.Client)
                      .WithMany(c => c.Accounts)
                      .HasForeignKey(a => a.ClientId);
            });
            
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasOne(t => t.Account)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.AccountId);
                entity.Property(t => t.TransactionType)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(t => t.Amount)
                      .HasPrecision(18, 2);
            });
        }
    }
}

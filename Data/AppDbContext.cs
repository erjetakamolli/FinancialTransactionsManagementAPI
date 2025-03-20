using Microsoft.EntityFrameworkCore;
using FinancialTransactionsManagementAPI.Models;

namespace FinancialTransactionsManagementAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfiguron relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Customer)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CustomerId);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransactionType)
                .HasConversion<string>(); // Konfiguron TransactionType ne string

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Status)
                .HasConversion<string>(); // Konfiguron status ne string

            // Konfiguron required fields
            modelBuilder.Entity<Customer>()
                .Property(c => c.FullName)
                .IsRequired();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Email)
                .IsRequired();
        }
    }
}
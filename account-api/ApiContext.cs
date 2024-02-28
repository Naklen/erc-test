using account_api.Models;
using Microsoft.EntityFrameworkCore;

namespace account_api
{
    public class ApiContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Resident> Residents { get; set; }

        public ApiContext(DbContextOptions<ApiContext> options) :
            base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasAlternateKey(a => a.AccountNumber);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using WebWallet.Models;

namespace WebWallet.Data
{
    public class WebWalletContext : DbContext
    {
        public WebWalletContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<BankAccount> BankAccount { get; set; }

        public DbSet<Transaction> Transaction { get; set; }
    }
}
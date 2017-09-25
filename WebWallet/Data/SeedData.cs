using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WebWallet.Data;

namespace WebWallet.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new WebWalletContext(
                serviceProvider.GetRequiredService<DbContextOptions<WebWalletContext>>()))
            {
                // Look for any bank accounts.
                //if (context.BankAccount.Any())
                //{
                //    return;   // DB has been seeded
                //}

                context.SaveChanges();
            }
        }
    }
}
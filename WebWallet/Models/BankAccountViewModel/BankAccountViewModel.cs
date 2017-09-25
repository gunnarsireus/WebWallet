using System.ComponentModel.DataAnnotations;
using WebWallet.Models;


namespace WebWallet.Models.BankAccountViewModel
{
    public class BankAccountViewModel: BankAccount
    {
        [Display(Name = "Användare")]
        public string Username { get; set; }

    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace WebWallet.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        [Display(Name = "Kontonummer")]
        public Guid BankAccountId { get; set; }
        [Display(Name = "Datum")]
        public string CreationTime { get; set; }
        [Display(Name = "Kommentar")]
        public string Comment { get; set; }
        [Display(Name = "Insättning")]
        [RegularExpression(@"^[0-9]+(\,([0-9]{1,2})?)?$", ErrorMessage = "{0} anges med upp till två decimaler. Ex: 4,75")]
        public string Deposit { get; set; }
        [Display(Name = "Uttag")]
        [RegularExpression(@"^[0-9]+(\,([0-9]{1,2})?)?$", ErrorMessage = "{0} anges med upp till två decimaler. Ex: 4,75")]
        public string Withdraw { get; set; }
    }
}

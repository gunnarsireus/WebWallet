using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebWallet.Models
{
    public class BankAccount
    {
        [Display(Name = "Kontonummer")]
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        [Display(Name = "Skapat datum")]
        public string CreationTime { get; set; }

        [Required]
        [Display(Name = "Ränta")]
        [RegularExpression(@"^(\+|\-|)[0-9]+(\,([0-9]{1,2})?)?$", ErrorMessage = "{0}n anges med upp till två decimaler. Ex: 4,75")]
        public string InterestRate { get; set; }
        [Display(Name = "Kommentar")]
        public string Comment { get; set; }
        [Required]
        [Display(Name = "Saldo")]
        [RegularExpression(@"^(\+|\-|)[0-9]+(\,([0-9]{1,2})?)?$", ErrorMessage = "{0}t anges med upp till två decimaler. Ex: 4,75")]
        public string Balance { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}

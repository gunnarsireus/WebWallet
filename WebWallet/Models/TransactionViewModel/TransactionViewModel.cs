using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebWallet.Models.TransactionViewModel
{
    public class TransactionViewModel: Transaction
    {
        [Display(Name = "Belopp")]
        [RegularExpression(@"^[0-9]+(\,([0-9]{1,2})?)?$", ErrorMessage = "{0} anges med upp till två decimaler. Ex: 4,75")]
        public string Amount { get; set; }
        [Display(Name = "Insättning eller uttag?")]
        public bool Deposition { get; set; }

    }
}

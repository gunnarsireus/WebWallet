using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using WebWallet.Data;
using WebWallet.Models;
using WebWallet.Models.TransactionViewModel;
using WebWallet.Services;

namespace WebWallet.Controllers
{
    public class TransactionController : Controller
    {
        private readonly WebWalletContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public TransactionController(WebWalletContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: Transaction
        public async Task<IActionResult> Index(string id)
        {

            var ownUserId = Guid.NewGuid(); //null Test to pass xUnit test where User=null
            if (User != null) ownUserId = new Guid(_userManager.GetUserId(User));
            var bankAccounts = await _context.BankAccount.Where(o => o.OwnerId == ownUserId).OrderBy(c => c.Comment).ToListAsync();
            if (bankAccounts.Any() && id == null)
            {
                id = bankAccounts[0].Id.ToString();
            }
            var selectList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Välj kontonummer",
                    Value = ""
                }
            };
            foreach (var bankAcount in bankAccounts)
            {
                selectList.Add(new SelectListItem
                {
                    Text = bankAcount.Comment,
                    Value = bankAcount.Id.ToString(),
                    Selected = bankAcount.Id.ToString() == id
                });
            }
            var transactions = new List<Transaction>();

            if (id != null)
            {
                var bankAccount = await _context.BankAccount.SingleOrDefaultAsync(o => o.Id.ToString() == id);
                transactions = await _context.Transaction.Where(o => o.BankAccountId == bankAccount.Id).ToListAsync();
            }


            var transactionListViewModel = new TransactionListViewModel
            {
                BankAccounts = selectList,
                Transactions = transactions
            };
            ViewBag.BankAccountId = id;
            ViewBag.Saldo = bankAccounts.SingleOrDefault(o => o.Id.ToString() == id)?.Balance;
            return View(transactionListViewModel);
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transaction
                .SingleOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create(string id)
        {
            if (TempData["CustomError"] != null)
            {
                ModelState.AddModelError(string.Empty, TempData["CustomError"].ToString());
            }
            if (id == null)
            {
                return View();
            }
            else
            {
                var iid = new Guid(id);
                var transactionViewModel = new TransactionViewModel()
                {
                    BankAccountId = iid,
                    Deposition = true
                };
                return View(transactionViewModel);
            }

        }

        // POST: Transaction/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BankAccountId,Comment,Deposition,Amount")] TransactionViewModel transactionViewModel)
        {
            if (ModelState.IsValid)
            {
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    BankAccountId = transactionViewModel.BankAccountId,
                    Comment = transactionViewModel.Comment,
                    CreationTime = DateTime.Now.ToString(new CultureInfo("se-SE")),
                    Deposit = transactionViewModel.Deposition ? transactionViewModel.Amount : "",
                    Withdraw = !transactionViewModel.Deposition ? transactionViewModel.Amount : ""
                };


                var bankAccount = await _context.BankAccount.SingleOrDefaultAsync(m => m.Id == transactionViewModel.BankAccountId);
                decimal oldBalance = decimal.Parse(bankAccount.Balance, new CultureInfo("se-SV"));
                if (transactionViewModel.Deposition)
                {
                    decimal deposit = decimal.Parse(transactionViewModel.Amount, new CultureInfo("se-SV"));
                    bankAccount.Balance = (oldBalance + deposit).ToString(new CultureInfo("se-SV"));
                }
                else
                {
                    decimal withdraw = decimal.Parse(transactionViewModel.Amount, new CultureInfo("se-SV"));
                    bankAccount.Balance = (oldBalance - withdraw).ToString(new CultureInfo("se-SV"));
                    if (decimal.Parse(bankAccount.Balance, new CultureInfo("se-SV")) < 0)
                    {
                        TempData["CustomError"] = "Saldot får ej bli negativt!";
                        return RedirectToAction("Create", new { id = transactionViewModel.BankAccountId });
                    }
                }
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { id = transactionViewModel.BankAccountId });
            }
            return View(transactionViewModel);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transaction.SingleOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }
            var transactionViewmodel = new TransactionViewModel
            {
                Id = transaction.Id,
                BankAccountId = transaction.BankAccountId,
                Comment = transaction.Comment,
                CreationTime = transaction.CreationTime,
                Amount = (transaction.Deposit == "") ? transaction.Withdraw : transaction.Deposit,
                Deposition = (transaction.Deposit != "")
            };
            return View(transactionViewmodel);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,BankAccountId,CreationTime,Comment,Deposit,Withdraw")] TransactionViewModel transactionViewModel)
        {
            if (id != transactionViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var oldTransaction = _context.Transaction.FirstOrDefault(o => o.Id == transactionViewModel.Id);
                    oldTransaction.Comment = transactionViewModel.Comment;
                    _context.Update(oldTransaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transactionViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", new { id = transactionViewModel.BankAccountId });
            }
            return View(transactionViewModel);
        }

        private bool TransactionExists(Guid id)
        {
            return _context.Transaction.Any(e => e.Id == id);
        }
    }
}

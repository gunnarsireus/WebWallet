using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebWallet.Data;
using WebWallet.Models;
using WebWallet.Models.BankAccountViewModel;
using WebWallet.Services;


namespace WebWallet.Controllers
{
    public class BankAccountController : Controller
    {
        private readonly WebWalletContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public BankAccountController(WebWalletContext context,
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

        // GET: BankAccount
        private string GetUserNameFromId(Guid id)
        {
            var users = _userManager.Users.ToList();
            return users.FirstOrDefault(o => o.Id == id.ToString())?.UserName;
        }
        public async Task<IActionResult> Index()
        {
            var ownUserId = Guid.NewGuid(); //null Test to pass xUnit test where User=null
            if (User != null) ownUserId = new Guid(_userManager.GetUserId(User));
            var bankAccounts = await _context.BankAccount.Where(o => o.OwnerId == ownUserId).OrderBy(c => c.Comment).ToListAsync();

            var bankAccountsViewModels = new List<BankAccountViewModel>();
            foreach (var bankAccount in bankAccounts)
            {
                bankAccountsViewModels.Add(new BankAccountViewModel
                {
                    Id = bankAccount.Id,
                    Username = GetUserNameFromId(bankAccount.OwnerId),
                    Balance = bankAccount.Balance,
                    Comment = bankAccount.Comment,
                    CreationTime = bankAccount.CreationTime,
                    InterestRate = bankAccount.InterestRate,
                    Transactions = bankAccount.Transactions
                });
            }
            return View(bankAccountsViewModels);
        }

        // GET: BankAccount/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccount
                .SingleOrDefaultAsync(m => m.Id == id);
            if (bankAccount == null)
            {
                return NotFound();
            }
            var bankAccountsViewModel = new BankAccountViewModel
            {
                Id = bankAccount.Id,
                Username = GetUserNameFromId(bankAccount.OwnerId),
                Balance = bankAccount.Balance,
                Comment = bankAccount.Comment,
                CreationTime = bankAccount.CreationTime,
                InterestRate = bankAccount.InterestRate,
                Transactions = bankAccount.Transactions
            };


            return View(bankAccountsViewModel);
        }

        // GET: BankAccount/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BankAccount/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InterestRate,Comment,Balance")] BankAccount bankAccount)
        {
            bankAccount.CreationTime = DateTime.Now.ToString(new CultureInfo("se-SE"));
            bankAccount.OwnerId = new Guid(_userManager.GetUserId(User));
            if (ModelState.IsValid)
            {
                bankAccount.Id = Guid.NewGuid();
                _context.Add(bankAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bankAccount);
        }

        // GET: BankAccount/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccount.SingleOrDefaultAsync(m => m.Id == id);
            if (bankAccount == null)
            {
                return NotFound();
            }
            var bankAccountsViewModel = new BankAccountViewModel
            {
                Id = bankAccount.Id,
                Username = GetUserNameFromId(bankAccount.OwnerId),
                Balance = bankAccount.Balance,
                Comment = bankAccount.Comment,
                CreationTime = bankAccount.CreationTime,
                InterestRate = bankAccount.InterestRate,
                Transactions = bankAccount.Transactions
            };


            return View(bankAccountsViewModel);
        }

        // POST: BankAccount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CreationTime,InterestRate,Comment,Balance")] BankAccountViewModel bankAccountViewModel)
        {
            if (id != bankAccountViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var oldbankAccount = _context.BankAccount.FirstOrDefault(o => o.Id == bankAccountViewModel.Id);
                oldbankAccount.Comment = bankAccountViewModel.Comment;
                oldbankAccount.InterestRate = bankAccountViewModel.InterestRate;
                try
                {
                    _context.Update(oldbankAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankAccountExists(oldbankAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bankAccountViewModel);
        }

        // GET: BankAccount/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccount = await _context.BankAccount
                .SingleOrDefaultAsync(m => m.Id == id);
            if (bankAccount == null)
            {
                return NotFound();
            }
            var bankAccountsViewModel = new BankAccountViewModel
            {
                Id = bankAccount.Id,
                Username = GetUserNameFromId(bankAccount.OwnerId),
                Balance = bankAccount.Balance,
                Comment = bankAccount.Comment,
                CreationTime = bankAccount.CreationTime,
                InterestRate = bankAccount.InterestRate,
                Transactions = bankAccount.Transactions
            };

            return View(bankAccountsViewModel);
        }

        // POST: BankAccount/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var bankAccount = await _context.BankAccount.SingleOrDefaultAsync(m => m.Id == id);
            _context.BankAccount.Remove(bankAccount);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BankAccountExists(Guid id)
        {
            return _context.BankAccount.Any(e => e.Id == id);
        }
    }
}

using AutoMapper;
using BankWebApp.Helpers;
using BankWebApp.Services;
using BankWebApp.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BankWebApp.Pages.Accounts
{
    [Authorize(Roles = "Cashier")]
    public class WithdrawalModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public WithdrawalViewModel Withdrawal { get; set; } = null!;

        [BindProperty]
        [Required(ErrorMessage = "Please enter an amount.")]
        [Range(100, 10000, ErrorMessage = "Please choose an amount between 100 and 10 000 kr.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public WithdrawalModel(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        public class WithdrawalViewModel
        {
            public AccountViewModel Account { get; set; } = null!;
            public int CustomerId { get; set; }
        }

        public IActionResult OnGet(int accountId, int customerId)
        {
            return Load(accountId, customerId) ? Page() : NotFound();
        }

        public IActionResult OnPost(int accountId, int customerId)
        {
            var account = _accountService.GetAccountForCustomer(accountId);
            if (account == null) return NotFound();

            if (decimal.Round(Amount, 2) != Amount)
                ModelState.AddModelError(nameof(Amount), "Use at most two decimals (öre).");
            if (account.Balance < Amount)
                ModelState.AddModelError(nameof(Amount), $"Insufficient funds. The balance is {BankFormat.Sek(account.Balance)}.");

            if (ModelState.IsValid)
            {
                _accountService.Withdraw(account, Amount);

                TempData["success"] = $"Withdrew {BankFormat.Sek(Amount)}.";

                return RedirectToPage("/Accounts/Transactions", new { accountId, customerId });
            }

            Load(accountId, customerId);
            return Page();
        }

        private bool Load(int accountId, int customerId)
        {
            var account = _accountService.GetAccountForCustomer(accountId);
            if (account == null) return false;

            Withdrawal = new WithdrawalViewModel { Account = _mapper.Map<AccountViewModel>(account), CustomerId = customerId };
            return true;
        }
    }
}

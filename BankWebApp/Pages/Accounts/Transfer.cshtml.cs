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
    public class TransferModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public TransferViewModel Transfer { get; set; } = null!;

        [BindProperty]
        [Required(ErrorMessage = "Please enter an amount.")]
        [Range(100, 10000, ErrorMessage = "Please choose an amount between 100 and 10 000 kr.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter an account number.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid account number.")]
        public int AccountToId { get; set; }

        public TransferModel(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        public class TransferViewModel
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
            var accountTo = _accountService.GetAccountForCustomer(AccountToId);
            if (account.Balance < Amount)
                ModelState.AddModelError(nameof(Amount), $"Insufficient funds. The balance is {BankFormat.Sek(account.Balance)}.");
            if (accountTo == null)
                ModelState.AddModelError(nameof(AccountToId), "There is no account with that number.");
            else if (accountTo.AccountId == account.AccountId)
                ModelState.AddModelError(nameof(AccountToId), "Choose a different account to transfer to.");

            if (ModelState.IsValid)
            {
                _accountService.Transfer(account, accountTo!, Amount);

                TempData["success"] = $"Transferred {BankFormat.Sek(Amount)} to account {BankFormat.AccountNumber(AccountToId)}.";

                return RedirectToPage("/Accounts/Transactions", new { accountId, customerId });
            }

            Load(accountId, customerId);
            return Page();
        }

        private bool Load(int accountId, int customerId)
        {
            var account = _accountService.GetAccountForCustomer(accountId);
            if (account == null) return false;

            Transfer = new TransferViewModel { Account = _mapper.Map<AccountViewModel>(account), CustomerId = customerId };
            return true;
        }
    }
}
